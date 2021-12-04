using System;
using IM.Service.Company.Analyzer.DataAccess.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.Models.Calculator;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public static class CalculatorService
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
                    .Select(x => x.Result)));
            var taskResultReport = Task.Run(() => ComputeSampleResult(data
                    .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Report)
                    .Select(x => x.Result)));
            var taskResultCoefficient = Task.Run(() => ComputeSampleResult(data
                    .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Coefficient)
                    .Select(x => x.Result)));

            Task.WhenAll(taskResultPrice, taskResultReport, taskResultCoefficient);

            var resultPrice = taskResultPrice.Result * 1000; //1000 - weight
            var resultReport = taskResultReport.Result;
            var resultCoefficient = taskResultCoefficient.Result;

            return new Rating
            {
                CompanyId = companyId,
                
                ResultPrice  = resultPrice,
                ResultReport = resultReport,
                ResultCoefficient = resultCoefficient,

                Result = ComputeSampleResult(new[] { resultPrice, resultReport, resultCoefficient }),
            };
        });


    public static Sample[] CompareSample(IEnumerable<Sample> sample)
    {
        var results = Array.Empty<Sample>();
        var cleanedValues = sample.Where(x => x.Value != 0).ToArray();

        if (cleanedValues.Length >= 2)
            results = ComputeValues(cleanedValues);

        return results;
    }
    public static decimal ComputeSampleResult(IEnumerable<decimal>? sample)
    {
        if (sample is null)
            return 0;

        var results = sample.ToArray();

        if (!results.Any())
            return 0;

        var zeroDeviationCount = results.Count(x => x != 0);
        var deviationCoefficient = (decimal)zeroDeviationCount / results.Length;
        return results.Average() * deviationCoefficient;
    }


    private static Sample[] ComputeValues(IReadOnlyList<Sample> sample)
    {
        var results = new Sample[sample.Count - 1];
        for (var i = 1; i < sample.Count; i++)
            results[i - 1] = new Sample
            {
                CompanyId = sample[i].CompanyId,
                Date = sample[i].Date,
                CompareTypes = sample[i].CompareTypes,

                Value = ComputeDeviationPercent(sample[i - 1].Value, sample[i].Value, sample[i].CompareTypes)
            };

        return results;
    }
    private static decimal ComputeDeviationPercent(decimal previousValue, decimal nextValue, CompareTypes compareTypes) =>
        (nextValue - previousValue) / Math.Abs(previousValue) * (short)compareTypes;
}