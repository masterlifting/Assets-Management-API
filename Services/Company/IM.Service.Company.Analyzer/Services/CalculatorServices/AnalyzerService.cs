using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class AnalyzerService
{
    private readonly IServiceScopeFactory scopeFactory;
    public AnalyzerService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task AnalyzeAsync()
    {
        var repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<AnalyzedEntity>>();
        var queryFilter = repository.GetQuery(x => x.StatusId == (byte)Statuses.Ready).Take(10);
        var data = await repository.GetSampleAsync(queryFilter);

        if (!data.Any() || !(await ChangeStatusAsync(Statuses.Processing, data, repository)))
            return;

        IImmutableList<string> ratingKeys;
        AnalyzedEntity[] computedData;
        try
        {
            computedData = await CalculateDataAsync(data);

            ratingKeys = computedData
                .Where(x => x.StatusId == (byte) Statuses.Computed)
                .GroupBy(x => x.CompanyId)
                .Select(x => x.Key)
                .ToImmutableList();
            
            foreach (var (analyzedEntity, result) in computedData
                         .Where(x => x.StatusId == (byte)Statuses.Starter)
                         .Join(data,
                             x => (x.CompanyId, x.AnalyzedEntityTypeId, x.Date),
                             y => (y.CompanyId, y.AnalyzedEntityTypeId, y.Date),
                             (x, y) => (x, y.Result)))
                analyzedEntity.Result = result;
        }
        catch
        {
            await ChangeStatusAsync(Statuses.Error, data, repository);
            return;
        }

        var (error, _) = await repository.CreateUpdateAsync(computedData, new AnalyzedEntityComparer(), nameof(AnalyzeAsync));

        if (error is null)
            await SetRatingAsync(ratingKeys, repository);
    }

    private async Task<bool> ChangeStatusAsync(Statuses status, AnalyzedEntity[] entities, Repository<AnalyzedEntity, DatabaseContext> repository)
    {
        foreach (var item in entities)
            item.StatusId = (byte)status;

        var (error, _) = await repository.CreateUpdateAsync(entities, new AnalyzedEntityComparer(), nameof(ChangeStatusAsync) + " to " + status);

        return error is null;
    }
    private async Task<AnalyzedEntity[]> CalculateDataAsync(IEnumerable<AnalyzedEntity> data)
    {
        var client = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<CompanyDataClient>();

        var calculators = new Dictionary<byte, IAnalyzerCalculator>
            {
                {(byte)EntityTypes.Price, new CalculatorPrice(client)},
                {(byte)EntityTypes.Report, new CalculatorReport(client)},
                {(byte)EntityTypes.Coefficient, new CalculatorCoefficient(scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<CalculatorCoefficient>>(), client)}
            };

        var tasks = data
            .GroupBy(x => x.AnalyzedEntityTypeId)
            .Select(x => calculators[x.Key].ComputeAsync(x.ToImmutableArray()));

        var result = await Task.WhenAll(tasks);

        return result.SelectMany(x => x).ToArray();
    }
    private async Task SetRatingAsync(IImmutableList<string> companyIds, Repository<AnalyzedEntity, DatabaseContext> repository)
    {
        List<Task<Rating>> ratingTasks = new(companyIds.Count);
        foreach (var companyId in companyIds)
        {
            var queryFilter = repository.GetQuery(x => x.CompanyId == companyId && x.StatusId == (byte)Statuses.Computed);
            var data = await repository.GetSampleAsync(queryFilter);
            ratingTasks.Add(CalculatorService.RatingCalculator.GetRatingAsync(companyId, data));
        }

        var rating = await Task.WhenAll(ratingTasks);

        var ratingRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Rating>>();
        await ratingRepository.CreateUpdateAsync(rating, new RatingComparer(), nameof(AnalyzeAsync) + '.' + nameof(SetRatingAsync));
    }
}