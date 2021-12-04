using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
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
        var analyzedEntityRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<AnalyzedEntity>>();
        var data = await analyzedEntityRepository.GetSampleAsync(x => x.StatusId == (byte)Statuses.Ready);

        if (!data.Any())
            return;

        foreach (var item in data)
            item.StatusId = (byte)Statuses.Processing;

        var (dataError, _) = await analyzedEntityRepository.UpdateAsync(data, nameof(AnalyzeAsync));

        if (dataError is not null)
            return;

        try
        {
            var ratingKeys = data
                .GroupBy(x => (x.CompanyId))
                .Select(x => x.Key)
                .ToArray();

            var calculatedData = await GetCalculatedDataAsync(data);

            foreach (var item in calculatedData)
                item.StatusId = (byte)Statuses.Completed;

            var (analyzedEntityerror, _) = await analyzedEntityRepository.CreateUpdateAsync(calculatedData, new AnalyzedEntityComparer(), nameof(AnalyzeAsync));

            foreach (var item in data)
                item.StatusId = (byte)Statuses.Completed;

            if (analyzedEntityerror is null)
            {
                var ratingTasks = ratingKeys
                    .Select(async companyId => await CalculatorService.GetRatingAsync(companyId, await analyzedEntityRepository.GetSampleAsync(z => z.CompanyId == companyId)));

                var calculatedRating = await Task.WhenAll(ratingTasks);

                var ratingRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RepositorySet<Rating>>();
                await ratingRepository.CreateUpdateAsync(calculatedRating, new RatingComparer(), nameof(AnalyzeAsync));
            }
            else
                foreach (var item in data)
                    item.StatusId = (byte)Statuses.Error;
        }
        catch
        {
            foreach (var item in data)
                item.StatusId = (byte)Statuses.Error;
        }

        await analyzedEntityRepository.UpdateAsync(data, nameof(AnalyzeAsync));
    }
    private async Task<AnalyzedEntity[]> GetCalculatedDataAsync(IEnumerable<AnalyzedEntity> data)
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
            .Select(x => calculators[x.Key].ComputeAsync(x));

        var result = await Task.WhenAll(tasks);

        return result.SelectMany(x => x).ToArray();
    }
}