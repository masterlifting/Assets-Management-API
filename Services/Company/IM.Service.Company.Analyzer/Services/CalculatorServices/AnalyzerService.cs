using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Analyzer.DataAccess;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Data;
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
        var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<AnalyzerService>>();
        var analyzedEntityRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<AnalyzedEntity>>();

        var data = await GetDataAsync(analyzedEntityRepository);
        var changeStatusResult = await ChangeStatusAsync(data, Statuses.Processing, analyzedEntityRepository);

        if (!changeStatusResult)
            throw new DataException("Change status failed!");

        var ratingData = await GetRatingDataAsync(data);
        var ratingDataRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<RatingData>>();
        await ratingDataRepository.CreateUpdateAsync(ratingData, new RatingDataComparer(), nameof(RatingData));


        var ratingService = new RatingService(ratingData);

        var ratingRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<Rating>>();
        var oldRating = await ratingRepository.GetSampleAsync();
        
        var newRating = ratingService.GetRating(oldRating);
        
        changeStatusResult = await ChangeStatusAsync(data, Statuses.Completed, analyzedEntityRepository);
        await ratingRepository.ReCreateAsync(newRating, nameof(Rating));
    }

    private static async Task<AnalyzedEntity[]> GetDataAsync(Repository<AnalyzedEntity, DatabaseContext> repository) =>
        await repository.GetSampleAsync(x => x.StatusId == (byte)Statuses.Ready);
    private static async Task<bool> ChangeStatusAsync(AnalyzedEntity[] data, Statuses status, Repository<AnalyzedEntity, DatabaseContext> repository)
    {
        foreach (var entity in data)
            entity.StatusId = (byte)status;

        var (error, _) = await repository.UpdateAsync(data, nameof(ChangeStatusAsync));

        if (error is not null)
            throw new DataException("Status was not changed. " + error);

        return true;
    }
    private async Task<RatingData[]> GetRatingDataAsync(IEnumerable<AnalyzedEntity> data)
    {
        var calculators = new Dictionary<byte, IAnalyzerCalculator>
        {
            {(byte)EntityTypes.Price, new PriceCalculator(scopeFactory)},
            {(byte)EntityTypes.Report, new ReportCalculator(scopeFactory)},
            {(byte)EntityTypes.Coefficient, new CoefficientCalculator(scopeFactory)}
        };

        var tasks = data
            .GroupBy(x => x.AnalyzedEntityTypeId)
            .Where(x => x.Key != (byte)EntityTypes.Coefficient && x.Key != (byte)EntityTypes.Price)
            .Select(x => calculators[x.Key].ComputeAsync(x));

        var result = await Task.WhenAll(tasks);

        return result.SelectMany(x => x).ToArray();
    }
}