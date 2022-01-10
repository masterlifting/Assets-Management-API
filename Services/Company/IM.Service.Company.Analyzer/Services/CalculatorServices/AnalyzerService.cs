using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class AnalyzerService
{
    private readonly IServiceScopeFactory scopeFactory;
    public AnalyzerService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task AnalyzeAsync()
    {
        var repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<AnalyzedEntity>>();

        var readyData = await repository.GetSampleAsync(x => x.StatusId == (byte)Statuses.Ready);

        if (!readyData.Any())
            return;

        if (!await SetStatusAsync(Statuses.Processing, readyData, repository))
            return;

        var notComputedData = await repository.GetSampleAsync(x => x.StatusId == (byte) Statuses.NotComputed);
        await repository.DeleteAsync(notComputedData, nameof(AnalyzeAsync));

        AnalyzedEntity[] computedData;

        try
        {
            computedData = await CalculateDataAsync(readyData);
        }
        catch
        {
            await SetStatusAsync(Statuses.Error, readyData, repository);
            return;
        }

        if (!computedData.Any())
        {
            await SetStatusAsync(Statuses.Ready, readyData, repository);
            return;
        }

        // Удаляю данные, по которым формировались расчеты т.к. они будут пересозданы все равно
        await repository.DeleteAsync(readyData, nameof(AnalyzeAsync));

        // Беру данные, с которых начинались сравнения
        var computingStartData = computedData
            .Where(x => x.StatusId == (byte)Statuses.NotComputed && x.Result == -1)
            .ToArray();

        // Перевожу их в статус - Вычесленные
        foreach (var item in computedData.Join(computingStartData,
                     x => new { x.CompanyId, x.AnalyzedEntityTypeId, x.Date },
                     y => new { y.CompanyId, y.AnalyzedEntityTypeId, y.Date },
                     (x, _) => x))
        {
            item.Result = 0;
            item.StatusId = (byte)Statuses.Computed;
        }

        // Ищу по ним совпадения в БД
        var companyIds = computingStartData.Select(x => x.CompanyId.ToUpperInvariant()).Distinct();
        var typeIds = computingStartData.Select(x => x.AnalyzedEntityTypeId).Distinct();
        var dates = computingStartData.Select(x => x.Date).Distinct();

        var dbStartData = await repository.GetQuery(x =>
                companyIds.Contains(x.CompanyId)
                && typeIds.Contains(x.AnalyzedEntityTypeId)
                && dates.Contains(x.Date))
            .ToArrayAsync();

        // Устанавливаю им результаты, которые были на момент расчета
        foreach (var (item, dbResult) in computedData.Join(dbStartData,
                     x => new { x.CompanyId, x.AnalyzedEntityTypeId, x.Date },
                     y => new { y.CompanyId, y.AnalyzedEntityTypeId, y.Date },
                    (x, y) => (x, y.Result)))
        {
            item.Result = dbResult;
        }

        // Сохраняю расчитанные данные
        var (error, _) = await repository.CreateUpdateAsync(computedData, new AnalyzedEntityComparer(), nameof(AnalyzeAsync));

        // Если сохранение прошло успешно, то пересчитываю рейтинг
        if (error is null)
            await SetRatingAsync();
    }

    private async Task<bool> SetStatusAsync(Statuses status, AnalyzedEntity[] entities, Repository<AnalyzedEntity, DatabaseContext> repository)
    {
        foreach (var item in entities)
            item.StatusId = (byte)status;

        var (error, _) = await repository.CreateUpdateAsync(entities, new AnalyzedEntityComparer(), nameof(SetStatusAsync) + " to " + status);

        return error is null;
    }
    private async Task<AnalyzedEntity[]> CalculateDataAsync(IEnumerable<AnalyzedEntity> data)
    {
        var client = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<CompanyDataClient>();
        var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<AnalyzerService>>();

        var calculatorData = new CalculatorData(client, data);

        var priceCalculateTask = Task.Run(() => CalculatorService.DataComparator.GetComparedSample(logger, calculatorData.Prices));
        var reportCalculateTask = Task.Run(() => CalculatorService.DataComparator.GetComparedSample(logger, calculatorData.Reports));
        var coefficientCalculateTask = Task.Run(() => CalculatorService.DataComparator.GetComparedSample(logger, calculatorData.Reports, calculatorData.Prices));

        var result = await Task.WhenAll(priceCalculateTask, reportCalculateTask, coefficientCalculateTask);

        return result.SelectMany(x => x).ToArray();
    }
    private Task SetRatingAsync()
    {
        var options = scopeFactory.CreateScope().ServiceProvider.GetRequiredService< IOptions < ServiceSettings > > ();
        var rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.CompanyAnalyzer, QueueEntities.Ratings, QueueActions.Call, DateTime.UtcNow.ToShortDateString());

        return Task.CompletedTask;
    }
}