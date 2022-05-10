using System.Collections.Immutable;
using System.Text;
using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Models.Services.Calculations.Rating;
using Microsoft.EntityFrameworkCore;
using static IM.Service.Common.Net.Enums;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Entity;

public sealed class RatingCalculator
{
    private readonly Repository<Company> companyRepository;
    private readonly Repository<Rating> ratingRepository;
    private readonly Repository<Price> priceRepository;
    private readonly Repository<Report> reportRepository;
    private readonly Repository<Coefficient> coefficientRepository;
    private readonly Repository<Dividend> dividendRepository;

    public RatingCalculator(
        Repository<Company> companyRepository,
        Repository<Rating> ratingRepository,
        Repository<Price> priceRepository,
        Repository<Report> reportRepository,
        Repository<Coefficient> coefficientRepository,
        Repository<Dividend> dividendRepository)
    {
        this.companyRepository = companyRepository;
        this.ratingRepository = ratingRepository;
        this.priceRepository = priceRepository;
        this.reportRepository = reportRepository;
        this.coefficientRepository = coefficientRepository;
        this.dividendRepository = dividendRepository;
    }

    public async Task ComputeRatingAsync()
    {
        var companies = await companyRepository.GetSampleAsync(x => ValueTuple.Create(x.Id, x.CountryId));

        List<Rating?> ratings = new(companies.Length);

        foreach (var (companyId, countryId) in companies)
            ratings.Add(await ComputeAsync(companyId, countryId));

        var result = ratings.Where(x => x is not null).Select(x => x!).ToArray();

        await ratingRepository.CreateUpdateDeleteAsync(result, new RatingComparer(), nameof(ComputeRatingAsync));
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

        var result = RatingCalculatorHelper.ComputeAverageResult(new[] { resultPrice, resultReport, resultCoefficient, resultDividend });

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

    public async Task<string> RecompareRatingAsync(CompareType compareType, string? companyId, int year = 0, int month = 0, int day = 0)
    {

        var priceFilter = DateFilter<Price>.GetFilter(compareType, companyId, null, year, month, day);
        var dividendFilter = DateFilter<Dividend>.GetFilter(compareType, companyId, null, year, month, day);
        var reportFilter = month == 0
            ? QuarterFilter<Report>.GetFilter(compareType, companyId, null, year)
            : QuarterFilter<Report>.GetFilter(compareType, companyId, null, year, LogicHelper.QuarterHelper.GetQuarter(month));
        var coefficientFilter = month == 0
            ? QuarterFilter<Coefficient>.GetFilter(compareType, companyId, null, year)
            : QuarterFilter<Coefficient>.GetFilter(compareType, companyId, null, year, LogicHelper.QuarterHelper.GetQuarter(month));

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
internal static class RatingCalculatorHelper
{
    /// <summary>
    /// Get results comparisions by rows. (rowIndex, result)
    /// </summary>
    /// <param name="sample"></param>
    /// <returns></returns>
    internal static Sample[] ComputeSampleResults(in Sample[] sample)
    {
        var cleanedSample = sample
            .Where(x => x.Value.HasValue)
            .ToArray();

        return cleanedSample.Length >= 2 ? ComputeValues(cleanedSample) : Array.Empty<Sample>();

        static Sample[] ComputeValues(in Sample[] sample)
        {
            var result = new Sample[sample.Length];
            result[0] = new Sample
            {
                Id = sample[0].Id,
                CompareType = sample[0].CompareType,
                Value = null
            };

            for (var i = 1; i < sample.Length; i++)
                result[i] = new Sample
                {
                    Id = sample[i].Id,
                    CompareType = sample[i].CompareType,
                    Value = ComputeDeviationPercent(sample[i - 1].Value!.Value, sample[i].Value!.Value, sample[i].CompareType)
                };

            return result;
        }

        static decimal ComputeDeviationPercent(decimal previousValue, decimal nextValue, CompareTypes compareTypes) =>
            (nextValue - previousValue) / Math.Abs(previousValue) * (short)compareTypes;
    }

    /// <summary>
    /// Get results comparisions by colums. (rowIndex, result)
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    internal static IDictionary<int, decimal?> ComputeSamplesResults(in Sample[][] samples)
    {
        var _samples = samples.Where(x => x.Any()).ToArray();

        if (!_samples.Any())
            return new Dictionary<int, decimal?>();

        var values = new Sample[_samples.Length];
        var rows = new Sample[_samples[0].Length][];

        for (var i = 0; i < _samples[0].Length; i++)
        {
            for (var j = 0; j < _samples.Length; j++)
                values[j] = new Sample
                {
                    Id = _samples[j][i].Id,
                    CompareType = _samples[j][i].CompareType,
                    Value = _samples[j][i].Value
                };

            rows[i] = ComputeSampleResults(values);
        }

        return rows
            .SelectMany(row => row)
            .GroupBy(row => row.Id)
            .ToImmutableDictionary(
                group => group.Key,
                group => ComputeAverageResult(group.Select(row => row.Value).ToArray()));
    }

    /// <summary>
    /// Compute average result. Depends on value without null.
    /// </summary>
    /// <param name="sample"></param>
    /// <returns></returns>
    internal static decimal? ComputeAverageResult(in decimal?[] sample)
    {
        if (!sample.Any())
            return null;

        var values = sample.Where(x => x is not null).ToArray();

        return values.Length != 0 ? values.Average() : null;
    }
}
