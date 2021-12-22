using System;
using IM.Service.Company.Analyzer.DataAccess.Entities;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.Models.Calculator;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public static class CalculatorService
{
    public static class RatingCalculator
    {
        public static Task<Rating> GetRatingAsync(string companyId, IEnumerable<AnalyzedEntity> data) =>
            GetRatingTask(companyId, data);
        public static Task<Rating[]> GetRatingAsync(IEnumerable<(string companyId, IEnumerable<AnalyzedEntity> companyData)> data) =>
            Task.WhenAll(data.Select(x => GetRatingTask(x.companyId, x.companyData)));
        public static Task<Rating[]> GetRatingAsync(IEnumerable<AnalyzedEntity> data) =>
            Task.WhenAll(data
                .GroupBy(x => x.CompanyId)
                .Select(x => GetRatingTask(x.Key, x)));
        private static Task<Rating> GetRatingTask(string companyId, IEnumerable<AnalyzedEntity> data) =>
            Task.Run(() =>
            {
                var taskResultPrice = Task.Run(() => ComputeSampleResult(data
                        .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Price)
                        .Select(x => x.Result)
                        .ToImmutableArray()));
                var taskResultReport = Task.Run(() => ComputeSampleResult(data
                        .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Report)
                        .Select(x => x.Result)
                        .ToImmutableArray()));
                var taskResultCoefficient = Task.Run(() => ComputeSampleResult(data
                        .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Coefficient)
                        .Select(x => x.Result)
                        .ToImmutableArray()));

                Task.WhenAll(taskResultPrice, taskResultReport, taskResultCoefficient);

                var resultPrice = taskResultPrice.Result * 1000; //1000 - weight
                var resultReport = taskResultReport.Result;
                var resultCoefficient = taskResultCoefficient.Result;

                return new Rating
                {
                    Result = ComputeSampleResult(new decimal[] { resultPrice, resultReport, resultCoefficient }),

                    CompanyId = companyId,

                    ResultPrice = resultPrice,
                    ResultReport = resultReport,
                    ResultCoefficient = resultCoefficient
                };
            });
    }

    public static Sample[] CompareSample(in IEnumerable<Sample> sample)
    {
        var cleanedSample = sample
            .Where(x => x.Value != 0)
            .ToImmutableList();

        return cleanedSample.Count >= 2
            ? ComputeValues(cleanedSample)
            : Array.Empty<Sample>();
    }
    public static Sample[][] CompareSampleByColumn(in Sample[][] samples, byte columnsCount)
    {
        var values = new Sample[samples.Length];
        var rows = new Sample[columnsCount][];

        for (var i = 0; i < columnsCount; i++)
        {
            for (var j = 0; j < samples.Length; j++)
                values[j] = new Sample
                {
                    Id = j,
                    CompareType = samples[j][i].CompareType,
                    Value = samples[j][i].Value
                };

            rows[i] = CompareSample(values);
        }

        return rows;
    }
    public static decimal ComputeSampleResult(IReadOnlyCollection<decimal> sample)
    {
        if (!sample.Any())
            return 0;

        var zeroDeviationCount = sample.Count(x => x != 0);
        var deviationCoefficient = (decimal)zeroDeviationCount / sample.Count;
        return sample.Average() * deviationCoefficient;
    }


    private static Sample[] ComputeValues(IReadOnlyList<Sample> sample)
    {
        var result = new Sample[sample.Count - 1];

        for (var i = 1; i < sample.Count; i++)
            result[i - 1] = new Sample
            {
                Id = sample[i].Id,
                CompareType = sample[i].CompareType,
                Value = ComputeDeviationPercent(sample[i - 1].Value, sample[i].Value, sample[i].CompareType)
            };

        return result;
    }
    private static decimal ComputeDeviationPercent(decimal previousValue, decimal nextValue, CompareTypes compareTypes) =>
        (nextValue - previousValue) / Math.Abs(previousValue) * (short)compareTypes;
}