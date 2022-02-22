using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Analyzer.Clients;
using IM.Service.Market.Analyzer.DataAccess;
using IM.Service.Market.Analyzer.DataAccess.Comparators;
using IM.Service.Market.Analyzer.DataAccess.Entities;
using IM.Service.Market.Analyzer.DataAccess.Repositories;
using IM.Service.Market.Analyzer.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Market.Analyzer.Enums;

namespace IM.Service.Market.Analyzer.Services.CalculatorServices;

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

        var notComputedData = await repository.GetSampleAsync(x => x.StatusId == (byte)Statuses.NotComputed);
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
            .Where(x => x.StatusId == (byte)Statuses.NotComputed)
            .ToArray();

        // Перевожу их в статус - Вычесленные
        foreach (var item in computedData.Join(computingStartData,
                     x => new { x.CompanyId, x.AnalyzedEntityTypeId, x.Date },
                     y => new { y.CompanyId, y.AnalyzedEntityTypeId, y.Date },
                     (x, _) => x))
        {
            item.Result = null;
            item.StatusId = (byte)Statuses.Computed;
        }

        // Ищу по ним совпадения в БД
        var companyIds = computingStartData
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var typeIds = computingStartData
            .GroupBy(x => x.AnalyzedEntityTypeId)
            .Select(x => x.Key);
        var dates = computingStartData
            .GroupBy(x => x.Date)
            .Select(x => x.Key);

        var dbStartData = await repository.GetQuery(x =>
                companyIds.Contains(x.CompanyId)
                && typeIds.Contains(x.AnalyzedEntityTypeId)
                && dates.Contains(x.Date))
            .ToArrayAsync();

        // Устанавливаю им результаты, которые были на момент расчета
        foreach (var (computed, oldResult) in computedData
                     .Join(computingStartData,
                         x => (x.CompanyId, x.AnalyzedEntityTypeId, x.Date),
                         y => (y.CompanyId, y.AnalyzedEntityTypeId, y.Date),
                         (x, _) => x)
                     .Join(dbStartData,
                         x => (x.CompanyId, x.AnalyzedEntityTypeId, x.Date),
                         y => (y.CompanyId, y.AnalyzedEntityTypeId, y.Date),
                         (x, y) => (x, y.Result)))
        {
            computed.Result = oldResult;
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

        List<Task<IEnumerable<AnalyzedEntity>>> tasks = new(calculatorData.Types.Count());

        foreach (var type in calculatorData.Types)
            switch (type)
            {
                case EntityTypes.Price:
                    tasks.Add(Task.Run(() => CalculatorService.DataComparator.GetComparedSample(logger, calculatorData.Prices)));
                    break;
                case EntityTypes.Report:
                    tasks.Add(Task.Run(() => CalculatorService.DataComparator.GetComparedSample(logger, calculatorData.Reports)));
                    break;
                case EntityTypes.Coefficient:
                    tasks.Add(Task.Run(() => CalculatorService.DataComparator.GetComparedSample(logger, calculatorData.Reports, calculatorData.Prices)));
                    break;
            }

        var result = await Task.WhenAll(tasks);

        return result.SelectMany(x => x).ToArray();
    }
    private Task SetRatingAsync()
    {
        var options = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IOptions<ServiceSettings>>();
        var rabbitConnectionString = options.Value.ConnectionStrings.Mq;

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketAnalyzer, QueueEntities.Ratings, QueueActions.Call, DateTime.UtcNow.ToShortDateString());

        return Task.CompletedTask;
    }
}