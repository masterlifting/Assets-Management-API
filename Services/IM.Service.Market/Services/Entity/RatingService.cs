using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.Helpers;
using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Shared.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Text;

using static IM.Service.Market.Enums;
using static IM.Service.Market.Services.Helpers.RatingHelper;
using static IM.Service.Shared.Enums;
using static IM.Service.Shared.Helpers.LogHelper;
using static IM.Service.Shared.Helpers.LogicHelper;

namespace IM.Service.Market.Services.Entity;

public class RatingService
{
    private readonly ILogger<RatingService> logger;
    private readonly Repository<Company> companyRepository;
    private readonly Repository<Rating> ratingRepository;
    private readonly Repository<Price> priceRepository;
    private readonly Repository<Report> reportRepository;
    private readonly Repository<Coefficient> coefficientRepository;
    private readonly Repository<Dividend> dividendRepository;

    public RatingService(
        ILogger<RatingService> logger,
        Repository<Company> companyRepository,
        Repository<Rating> ratingRepository,
        Repository<Price> priceRepository,
        Repository<Report> reportRepository,
        Repository<Coefficient> coefficientRepository,
        Repository<Dividend> dividendRepository)
    {
        this.logger = logger;
        this.companyRepository = companyRepository;
        this.ratingRepository = ratingRepository;
        this.priceRepository = priceRepository;
        this.reportRepository = reportRepository;
        this.coefficientRepository = coefficientRepository;
        this.dividendRepository = dividendRepository;
    }

    public async Task ComputeAsync()
    {
        var companies = await companyRepository.GetSampleAsync(x => ValueTuple.Create(x.Id, x.CountryId));

        List<Rating?> ratings = new(companies.Length);

        foreach (var (companyId, countryId) in companies)
            ratings.Add(await ComputeAsync(companyId, countryId));

        var result = ratings.Where(x => x is not null).Select(x => x!).ToArray();

        await ratingRepository.CreateUpdateDeleteAsync(result, new RatingComparer(), nameof(ComputeAsync));
    }
    public async Task<bool> CompareAsync<T>(IServiceProvider serviceProvider) where T : class, IRating
    {
        var repository = serviceProvider.GetRequiredService<Repository<T>>();
        var statusChanger = new StatusChanger<T>(repository);
        var readyData = await repository.GetSampleAsync(x => x.StatusId == (byte)Statuses.Ready || x.StatusId == (byte)Statuses.Error);

        if (!readyData.Any())
            return false;

        try
        {
            await statusChanger.SetStatusRangeAsync(readyData, Statuses.Computing);
            var computedData = (await RatingComparator.GetComparedSampleAsync(repository, readyData)).ToArray();
            await repository.UpdateRangeAsync(computedData, nameof(CompareAsync));
            return true;
        }
        catch(Exception exception)
        {
            await statusChanger.SetStatusRangeAsync(readyData, Statuses.Error);
            logger.LogError(nameof(CompareAsync), exception);
            return false;
        }
    }
    public async Task<string> RecompareAsync(CompareType compareType, string? companyId, int year = 0, int month = 0, int day = 0)
    {
        var priceFilter = DateFilter<Price>.GetFilter(compareType, companyId, null, year, month, day);
        var dividendFilter = DateFilter<Dividend>.GetFilter(compareType, companyId, null, year, month, day);
        var reportFilter = month == 0
            ? QuarterFilter<Report>.GetFilter(compareType, companyId, null, year)
            : QuarterFilter<Report>.GetFilter(compareType, companyId, null, year, QuarterHelper.GetQuarter(month));
        var coefficientFilter = month == 0
            ? QuarterFilter<Coefficient>.GetFilter(compareType, companyId, null, year)
            : QuarterFilter<Coefficient>.GetFilter(compareType, companyId, null, year, QuarterHelper.GetQuarter(month));

        var prices = await priceRepository.GetSampleAsync(priceFilter.Expression);
        var dividends = await dividendRepository.GetSampleOrderedAsync(dividendFilter.Expression, x => x.Date);
        var reports = await reportRepository.GetSampleOrderedAsync(reportFilter.Expression, x => x.Year, x => x.Quarter);
        var coefficients = await coefficientRepository.GetSampleOrderedAsync(coefficientFilter.Expression, x => x.Year, x => x.Quarter);

        var result = new StringBuilder(500);
        result.Append(await SetRatingComparisionTask(priceRepository, prices, x => x.Date));
        result.Append(await SetRatingComparisionTask(reportRepository, reports, x => x.Year, x => x.Quarter));
        result.Append(await SetRatingComparisionTask(dividendRepository, dividends, x => x.Date));
        result.Append(await SetRatingComparisionTask(coefficientRepository, coefficients, x => x.Year, x => x.Quarter));

        return result.ToString();
    }
    
    private async Task<Rating?> ComputeAsync(string companyId, byte countryId)
    {
        var sourceId = countryId == (byte)Countries.Rus ? (byte)Sources.Moex : (byte)Sources.Tdameritrade;

        var priceSum = await priceRepository.Where(x =>
                x.CompanyId == companyId
                && x.SourceId == sourceId
                && x.StatusId == (byte)Statuses.Computed
                && x.Result.HasValue)
            .SumAsync(x => x.Result);

        var reportSum = await reportRepository.Where(x =>
                x.CompanyId == companyId
                && x.SourceId == (byte)Sources.Investing
                && x.StatusId == (byte)Statuses.Computed
                && x.Result.HasValue)
            .SumAsync(x => x.Result);

        var coefficientSum = await coefficientRepository.Where(x =>
                x.CompanyId == companyId
                && x.SourceId == (byte)Sources.Investing
                && x.StatusId == (byte)Statuses.Computed
                && x.Result.HasValue)
            .SumAsync(x => x.Result);

        var dividendSum = await dividendRepository.Where(x =>
                x.CompanyId == companyId
                && x.SourceId == (byte)Sources.Yahoo
                && x.StatusId == (byte)Statuses.Computed
                && x.Result.HasValue)
            .SumAsync(x => x.Result);

        var resultPrice = priceSum * 10 / 1000;
        var resultReport = reportSum / 1000;
        var resultCoefficient = coefficientSum / 1000;
        var resultDividend = dividendSum / 1000;

        var result = RatingComputer.ComputeAverageResult(new[] { resultPrice, resultReport, resultCoefficient, resultDividend });

        return result == 0 ? null : new Rating
        {
            Result = result,

            CompanyId = companyId,

            ResultPrice = resultPrice,
            ResultReport = resultReport,
            ResultCoefficient = resultCoefficient,
            ResultDividend = resultDividend
        };
    }
    private static async Task<string> SetRatingComparisionTask<T, TSelector>(Repository<T, DatabaseContext> repository, T[] data, Func<T, TSelector> orderSelector, Func<T, TSelector>? orderSelector2 = null) where T : class, IRating, IPeriod
    {
        if (!data.Any())
            return $"\nData for {typeof(T).Name}s not found; ";

        var groupedData = data.GroupBy(x => x.CompanyId).ToArray();
        var dataResult = new List<T>(groupedData.Length);

        foreach (var group in groupedData)
        {
            var firstData = orderSelector2 is null
                ? group.OrderBy(orderSelector.Invoke).First()
                : group.OrderBy(orderSelector.Invoke).ThenBy(orderSelector2.Invoke).First();
            firstData.StatusId = (byte)Statuses.Ready;
            dataResult.Add(firstData);
        }

        await repository.UpdateRangeAsync(dataResult, "Recalculating rating");

        return $"\nRecompare {typeof(T).Name}s data for : {string.Join("; ", dataResult.Select(x => x.CompanyId))} is start; ";
    }
}
