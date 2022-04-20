using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Services.Calculations.Rating;

using Microsoft.EntityFrameworkCore;

using System.Collections.Immutable;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Calculations;

public class RatingCalculator
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

        List<Rating> ratings = new(companies.Length);

        foreach (var (companyId, countryId) in companies)
            ratings.Add(await ComputeAsync(companyId, countryId));

        await ratingRepository.CreateUpdateDeleteAsync(ratings, new RatingComparer(), nameof(ComputeRatingAsync));
    }
    private async Task<Rating> ComputeAsync(string companyId, byte countryId)
    {
        var sourceId = countryId == (byte)Countries.Rus ? (byte)Sources.Moex : (byte)Sources.Tdameritrade;

        var priceSum = await priceRepository.GetQuery(x =>
                    x.CompanyId == companyId
                    && x.SourceId == sourceId
                    && x.StatusId == (byte)Statuses.Computed
                    && x.Result.HasValue)
                .SumAsync(x => x.Result);

        var reportSum = await reportRepository.GetQuery(x =>
                    x.CompanyId == companyId
                    && x.SourceId == (byte)Sources.Investing
                    && x.StatusId == (byte)Statuses.Computed
                    && x.Result.HasValue)
                .SumAsync(x => x.Result);

        var coefficientSum = await coefficientRepository.GetQuery(x =>
                    x.CompanyId == companyId
                    && x.SourceId == (byte)Sources.Investing
                    && x.StatusId == (byte)Statuses.Computed
                    && x.Result.HasValue)
                .SumAsync(x => x.Result);

        var dividendSum = await dividendRepository.GetQuery(x =>
                    x.CompanyId == companyId
                    && x.SourceId == (byte)Sources.Yahoo
                    && x.StatusId == (byte)Statuses.Computed
                    && x.Result.HasValue)
                .SumAsync(x => x.Result);

        var resultPrice = priceSum * 10 / 1000;
        var resultReport = reportSum / 1000;
        var resultCoefficient = coefficientSum / 1000;
        var resultDividend = dividendSum / 1000;

        return new Rating
        {
            Result = RatingCalculatorHelper.ComputeAverageResult(new[] { resultPrice, resultReport, resultCoefficient, resultDividend }),

            CompanyId = companyId,

            ResultPrice = resultPrice,
            ResultReport = resultReport,
            ResultCoefficient = resultCoefficient,
            ResultDividend = resultDividend
        };
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
