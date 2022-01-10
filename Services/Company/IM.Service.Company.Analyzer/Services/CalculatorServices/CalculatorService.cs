using System;
using IM.Service.Company.Analyzer.DataAccess.Entities;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Models.Calculator;
using Microsoft.Extensions.Logging;
using static IM.Service.Common.Net.CommonHelper;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public static class CalculatorService
{
    public static class DataComparator
    {
        public static IEnumerable<AnalyzedEntity> GetComparedSample(ILogger<AnalyzerService> logger, in IEnumerable<PriceGetDto> dto) =>
            dto
                .GroupBy(x => x.Ticker)
                .SelectMany(x =>
                {
                    var companyOrderedData = x
                        .OrderBy(y => y.Date)
                        .ToImmutableArray();

                    var companySample = companyOrderedData
                        .Select((price, index) => new Sample { Id = index, CompareType = CompareTypes.Asc, Value = price.ValueTrue })
                        .ToArray();

                    var computedResults = ComputeHelper
                        .ComputeSampleResults(companySample)
                        .ToImmutableDictionary(y => y.Id, z => z.Value);

                    //check deviation
                    //foreach (var index in computedResults.Where(y => Math.Abs(y.Value) > 50).Select(y => y.Key))
                    //    logger.LogWarning(LogEvents.Processing,"Deviation of price > 50%! '{ticker}' at '{date}'",companyOrderedData[index].Ticker, companyOrderedData[index].Date.ToShortDateString());

                    if (!computedResults.Any())
                        return companyOrderedData
                            .Select(price => new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = price.Date,
                                AnalyzedEntityTypeId = (byte)EntityTypes.Price,
                                StatusId = (byte)Statuses.NotComputed,
                                Result = 0
                            });

                    var result = companyOrderedData
                            .Select((price, index) =>
                            {
                                var isComputed = computedResults.ContainsKey(index);

                                return new AnalyzedEntity
                                {
                                    CompanyId = x.Key,
                                    Date = price.Date,
                                    AnalyzedEntityTypeId = (byte)EntityTypes.Price,
                                    StatusId = isComputed
                                        ? (byte)Statuses.Computed
                                        : (byte)Statuses.NotComputed,
                                    Result = isComputed
                                        ? computedResults[index]
                                        : 0
                                };
                            })
                            .ToImmutableArray();

                    var startIndex = computedResults.OrderBy(y => y.Key).First().Key;
                    result[startIndex].StatusId = (byte)Statuses.NotComputed;
                    result[startIndex].Result = -1;

                    return result;
                });
        public static IEnumerable<AnalyzedEntity> GetComparedSample(ILogger<AnalyzerService> logger, in IEnumerable<ReportGetDto> dto) =>
            dto
                .GroupBy(x => x.Ticker)
                .SelectMany(x =>
                {
                    var companyOrderedData = x
                        .OrderBy(y => y.Year)
                        .ThenBy(y => y.Quarter)
                        .ToImmutableArray();

                    var companySamples = companyOrderedData
                        .Select((report, index) => new Sample[]
                            {
                                new ()  { Id = index, CompareType = CompareTypes.Asc, Value = report.Revenue ?? 0 }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.ProfitNet ?? 0 }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.ProfitGross ?? 0 }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.Asset ?? 0 }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.Turnover ?? 0 }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.ShareCapital ?? 0 }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.CashFlow ?? 0 }
                                ,new () { Id = index, CompareType = CompareTypes.Desc, Value = report.Obligation ?? 0 }
                                ,new () { Id = index, CompareType = CompareTypes.Desc, Value = report.LongTermDebt ?? 0 }
                            })
                        .ToArray();

                    var computedResults = ComputeHelper.ComputeSamplesResults(companySamples);

                    //check deviation
                    foreach (var index in computedResults.Where(y => Math.Abs(y.Value) > 50).Select(y => y.Key))
                        logger.LogWarning(LogEvents.Processing, "Deviation of report > 50%! '{ticker}' at Year: '{year}' Quarter: '{quarter}'", companyOrderedData[index].Ticker, companyOrderedData[index].Year, companyOrderedData[index].Quarter);

                    if (!computedResults.Any())
                        return companyOrderedData
                            .Select(report => new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = QuarterHelper.ToDateTime(report.Year, report.Quarter),
                                AnalyzedEntityTypeId = (byte)EntityTypes.Report,
                                StatusId = (byte)Statuses.NotComputed,
                                Result = 0
                            });

                    var result = companyOrderedData
                            .Select((report, index) =>
                            {
                                var isComputed = computedResults.ContainsKey(index);

                                return new AnalyzedEntity
                                {
                                    CompanyId = x.Key,
                                    Date = QuarterHelper.ToDateTime(report.Year, report.Quarter),
                                    AnalyzedEntityTypeId = (byte)EntityTypes.Report,
                                    StatusId = isComputed ? (byte)Statuses.Computed : (byte)Statuses.NotComputed,
                                    Result = isComputed ? computedResults[index] : 0
                                };
                            })
                            .ToImmutableArray();

                    var startIndex = computedResults.OrderBy(y => y.Key).First().Key;
                    result[startIndex].StatusId = (byte)Statuses.NotComputed;
                    result[startIndex].Result = -1;

                    return result;
                });
        public static IEnumerable<AnalyzedEntity> GetComparedSample(ILogger<AnalyzerService> logger, in IEnumerable<ReportGetDto> reportsDto, IEnumerable<PriceGetDto>? pricesDto) =>
            reportsDto
                .GroupBy(x => x.Ticker)
                .SelectMany(x =>
                {
                    var companyOrderedData = x
                        .OrderBy(y => y.Year)
                        .ThenBy(y => y.Quarter)
                        .ToImmutableArray();

                    var companySamples = companyOrderedData
                        .Select((report, index) =>
                        {
                            var firstMonth = QuarterHelper.GetFirstMonth(report.Quarter);
                            var lastMonth = QuarterHelper.GetLastMonth(report.Quarter);

                            var firstDate = new DateTime(report.Year, firstMonth, 1);
                            var lastDate = new DateTime(report.Year, lastMonth, 28);

                            var price = pricesDto?
                                .Where(price => price.Date >= firstDate && price.Date <= lastDate)
                                .OrderBy(price => price.Date)
                                .LastOrDefault();

                            try
                            {
                                var coefficient = ComputeHelper.ComputeCoefficient(report, price);

                                return new Sample[]
                                {
                                        new() {Id = index, CompareType = CompareTypes.Desc, Value = coefficient.Pe},
                                        new() {Id = index, CompareType = CompareTypes.Desc, Value = coefficient.Pb},
                                        new() {Id = index, CompareType = CompareTypes.Desc, Value = coefficient.DebtLoad},
                                        new() {Id = index, CompareType = CompareTypes.Asc, Value = coefficient.Profitability},
                                        new() {Id = index, CompareType = CompareTypes.Asc, Value = coefficient.Roa},
                                        new() {Id = index, CompareType = CompareTypes.Asc, Value = coefficient.Roe},
                                        new() {Id = index, CompareType = CompareTypes.Asc, Value = coefficient.Eps}
                                };
                            }
                            catch
                            {
                                return Array.Empty<Sample>();
                            }
                        })
                        .ToArray();

                    var computedResults = ComputeHelper.ComputeSamplesResults(companySamples);

                    //check deviation
                    //foreach (var index in computedResults.Where(y => Math.Abs(y.Value) > 50).Select(y => y.Key))
                    //    logger.LogWarning(LogEvents.Processing, "Deviation of coefficient > 50%! '{ticker}' at Year: '{year}' Quarter: '{quarter}'", companyOrderedData[index].Ticker, companyOrderedData[index].Year, companyOrderedData[index].Quarter);

                    if (!computedResults.Any())
                        return companyOrderedData
                            .Select(report => new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = QuarterHelper.ToDateTime(report.Year, report.Quarter),
                                AnalyzedEntityTypeId = (byte)EntityTypes.Coefficient,
                                StatusId = (byte)Statuses.NotComputed,
                                Result = 0
                            });

                    var comparedResult = companyOrderedData
                        .Select((report, index) =>
                        {
                            var isComputed = computedResults.ContainsKey(index);

                            return new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = QuarterHelper.ToDateTime(report.Year, report.Quarter),
                                AnalyzedEntityTypeId = (byte)EntityTypes.Coefficient,
                                StatusId = isComputed ? (byte)Statuses.Computed : (byte)Statuses.NotComputed,
                                Result = isComputed ? computedResults[index] : 0
                            };
                        })
                        .ToImmutableArray();

                    var startIndex = computedResults.OrderBy(y => y.Key).First().Key;
                    comparedResult[startIndex].StatusId = (byte)Statuses.NotComputed;
                    comparedResult[startIndex].Result = -1;

                    return comparedResult;
                });
    }
    public static class RatingHelper
    {
        public static Task<Rating> GetRatingAsync(string companyId, IEnumerable<AnalyzedEntity> data) =>
            Task.Run(() =>
            {
                var taskResultPrice = Task.Run(() => data
                        .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Price)
                        .Sum(x => x.Result));
                var taskResultReport = Task.Run(() => data
                        .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Report)
                        .Sum(x => x.Result));
                var taskResultCoefficient = Task.Run(() => data
                        .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Coefficient)
                        .Sum(x => x.Result));

                Task.WhenAll(taskResultPrice, taskResultReport, taskResultCoefficient);

                var resultPrice = taskResultPrice.Result * 10 / 1000;
                var resultReport = taskResultReport.Result / 1000;
                var resultCoefficient = taskResultCoefficient.Result / 1000;

                return new Rating
                {
                    Result = ComputeHelper.ComputeAverageResult(new[] { resultPrice, resultReport, resultCoefficient }),

                    CompanyId = companyId,

                    ResultPrice = resultPrice,
                    ResultReport = resultReport,
                    ResultCoefficient = resultCoefficient
                };
            });
    }
    private static class ComputeHelper
    {
        /// <summary>
        /// Compute report coefficients
        /// </summary>
        /// <param name="report"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        /// <exception cref="ArithmeticException"></exception>
        public static Coefficient ComputeCoefficient(ReportGetDto report, PriceGetDto? price)
        {
            try
            {
                if (report.Multiplier <= 0 || price?.StockVolume is null or <= 0)
                    throw new ArgumentException($"{nameof(report.Multiplier)} or {nameof(price.StockVolume)} is incorrect");

                var profitNet = report.ProfitNet ?? throw new ArgumentNullException($"{nameof(report.ProfitNet)} is null");
                var revenue = report.Revenue ?? throw new ArgumentNullException($"{nameof(report.Revenue)} is null");
                var asset = report.Asset ?? throw new ArgumentNullException($"{nameof(report.Asset)} is null");
                var shareCapital = report.ShareCapital ?? throw new ArgumentNullException($"{nameof(report.ShareCapital)} is null");
                var obligation = report.Obligation ?? throw new ArgumentNullException($"{nameof(report.Obligation)} is null");
                var eps = profitNet * report.Multiplier / price.StockVolume.Value;

                return new()
                {
                    Eps = eps,
                    Profitability = (profitNet / revenue + revenue / asset) * 0.5m,
                    Roa = profitNet / asset * 100,
                    Roe = profitNet / shareCapital * 100,
                    DebtLoad = obligation / asset,
                    Pe = price.Value / eps,
                    Pb = price.Value * price.StockVolume.Value / ((asset - obligation) * report.Multiplier),
                };
            }
            catch (Exception exception)
            {
                throw new ArithmeticException(exception.InnerException?.Message ?? exception.Message);
            }
        }

        /// <summary>
        /// Get results comparisions by rows. (rowIndex, result)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static Sample[] ComputeSampleResults(in Sample[] sample)
        {
            var cleanedSample = sample
                .Where(x => x.Value != 0)
                .ToArray();

            return cleanedSample.Length >= 2
                ? ComputeValues(cleanedSample)
                : Array.Empty<Sample>();
        }

        /// <summary>
        /// Get results comparisions by colums. (rowIndex, result)
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static IDictionary<int, decimal> ComputeSamplesResults(in Sample[][] samples)
        {
            var _samples = samples.Where(x => x.Any()).ToArray();

            if (!_samples.Any())
                return new Dictionary<int, decimal>();

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
                .ToImmutableDictionary(group => group.Key, group => ComputeAverageResult(group
                    .Select(row => row.Value)
                    .ToArray()));
        }

        /// <summary>
        /// Compute average result. Depends on value witout zero.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static decimal ComputeAverageResult(in decimal[] sample)
        {
            if (!sample.Any())
                return 0;

            var zeroDeviationCount = sample.Count(x => x != 0);
            var deviationCoefficient = (decimal)zeroDeviationCount / sample.Length;
            return sample.Average() * deviationCoefficient;
        }

        private static Sample[] ComputeValues(in Sample[] sample)
        {
            var result = new Sample[sample.Length];
            result[0] = new Sample
            {
                Id = sample[0].Id,
                CompareType = sample[0].CompareType,
                Value = 0
            };

            for (var i = 1; i < sample.Length; i++)
                result[i] = new Sample
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
}