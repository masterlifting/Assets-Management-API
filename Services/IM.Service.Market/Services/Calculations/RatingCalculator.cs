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
    private readonly IServiceScopeFactory scopeFactory;
    public RatingCalculator(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task ComputeRatingAsync()
    {
        var companyRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Company>>();
        var ratingRepository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Rating>>();

        var companies = await companyRepository.GetSampleAsync(x => ValueTuple.Create(x.Id, x.CountryId));
        var ratingTasks = companies.Select(x => ComputeAsync(x.Item1, x.Item2));
        var ratings = await Task.WhenAll(ratingTasks);

        await ratingRepository.CreateUpdateDeleteAsync(ratings, new RatingComparer(), nameof(ComputeRatingAsync));
    }
    private Task<Rating> ComputeAsync(string companyId, byte countryId) =>
        Task.Run(async () =>
        {
            var taskResultPrice = Task.Run(() =>
            {
                var repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Price>>();
                var sourceId = countryId == (byte) Countries.Rus ? (byte) Sources.Moex : (byte) Sources.Tdameritrade;
                
                return repository.GetQuery(x =>
                        x.CompanyId == companyId
                        && x.SourceId == sourceId
                        && x.StatusId == (byte) Statuses.Computed
                        && x.Result.HasValue)
                    .SumAsync(x => x.Result);
            });
            var taskResultReport = Task.Run(() =>
            {
                var repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Report>>();
                return repository.GetQuery(x =>
                        x.CompanyId == companyId
                        && x.SourceId == (byte)Sources.Investing
                        && x.StatusId == (byte)Statuses.Computed
                        && x.Result.HasValue)
                    .SumAsync(x => x.Result);
            });
            var taskResultCoefficient = Task.Run(() =>
            {
                var repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Coefficient>>();
                return repository.GetQuery(x =>
                        x.CompanyId == companyId
                        && x.SourceId == (byte)Sources.Investing
                        && x.StatusId == (byte)Statuses.Computed
                        && x.Result.HasValue)
                    .SumAsync(x => x.Result);
            });
            var taskResultDividend = Task.Run(() =>
            {
                var repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Dividend>>();
                return repository.GetQuery(x =>
                        x.CompanyId == companyId
                        && x.SourceId == (byte)Sources.Yahoo
                        && x.StatusId == (byte)Statuses.Computed
                        && x.Result.HasValue)
                    .SumAsync(x => x.Result);
            });

            await Task.WhenAll(taskResultPrice, taskResultReport, taskResultCoefficient, taskResultDividend);

            var resultPrice = taskResultPrice.Result * 10 / 1000;
            var resultReport = taskResultReport.Result / 1000;
            var resultCoefficient = taskResultCoefficient.Result / 1000;
            var resultDividend = taskResultDividend.Result / 1000;

            return new Rating
            {
                Result = RatingCalculatorHelper.ComputeAverageResult(new[] { resultPrice, resultReport, resultCoefficient, resultDividend }),

                CompanyId = companyId,

                ResultPrice = resultPrice,
                ResultReport = resultReport,
                ResultCoefficient = resultCoefficient,
                ResultDividend = resultDividend
            };
        });
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
