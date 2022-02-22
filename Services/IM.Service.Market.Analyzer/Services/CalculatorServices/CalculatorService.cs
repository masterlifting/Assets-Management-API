using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Market.Analyzer.DataAccess.Entities;
using IM.Service.Market.Analyzer.Models.Calculator;
using Microsoft.Extensions.Logging;
using static IM.Service.Common.Net.Helper;
using static IM.Service.Market.Analyzer.Enums;

namespace IM.Service.Market.Analyzer.Services.CalculatorServices;

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
                    var deviation = computedResults
                        .Where(y => y.Value.HasValue && Math.Abs(y.Value.Value) > 50)
                        .Select(y => y.Key)
                        .ToArray();
                    foreach (var index in deviation)
                        logger.LogWarning(LogEvents.Processing, "Deviation of price > 50%! '{ticker}' at '{date}'", companyOrderedData[index].Ticker, companyOrderedData[index].Date.ToShortDateString());

                    if (!computedResults.Any())
                        return companyOrderedData
                            .Select(price => new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = price.Date,
                                AnalyzedEntityTypeId = (byte)EntityTypes.Price,
                                StatusId = (byte)Statuses.NotComputed
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
                                    StatusId = isComputed ? (byte)Statuses.Computed : (byte)Statuses.NotComputed,
                                    Result = isComputed ? computedResults[index] : null
                                };
                            })
                            .ToImmutableArray();

                    var startIndex = computedResults.OrderBy(y => y.Key).First().Key;
                    result[startIndex].StatusId = (byte)Statuses.NotComputed;

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
                                new ()  { Id = index, CompareType = CompareTypes.Asc, Value = report.Revenue }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.ProfitNet }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.ProfitGross }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.Asset }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.Turnover }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.ShareCapital }
                                ,new () { Id = index, CompareType = CompareTypes.Asc, Value = report.CashFlow }
                                ,new () { Id = index, CompareType = CompareTypes.Desc, Value = report.Obligation }
                                ,new () { Id = index, CompareType = CompareTypes.Desc, Value = report.LongTermDebt }
                            })
                        .ToArray();

                    var computedResults = ComputeHelper.ComputeSamplesResults(companySamples);

                    //check deviation
                    var deviation = computedResults
                        .Where(y => y.Value.HasValue && Math.Abs(y.Value.Value) > 100)
                        .Select(y => y.Key)
                        .ToArray();
                    foreach (var index in deviation)
                        logger.LogWarning(LogEvents.Processing, "Deviation of report > 100%! '{ticker}' at Year: '{year}' Quarter: '{quarter}'", companyOrderedData[index].Ticker, companyOrderedData[index].Year, companyOrderedData[index].Quarter);

                    if (!computedResults.Any())
                        return companyOrderedData
                            .Select(report => new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = QuarterHelper.ToDate(report.Year, report.Quarter),
                                AnalyzedEntityTypeId = (byte)EntityTypes.Report,
                                StatusId = (byte)Statuses.NotComputed
                            });

                    var result = companyOrderedData
                            .Select((report, index) =>
                            {
                                var isComputed = computedResults.ContainsKey(index);

                                return new AnalyzedEntity
                                {
                                    CompanyId = x.Key,
                                    Date = QuarterHelper.ToDate(report.Year, report.Quarter),
                                    AnalyzedEntityTypeId = (byte)EntityTypes.Report,
                                    StatusId = isComputed ? (byte)Statuses.Computed : (byte)Statuses.NotComputed,
                                    Result = isComputed ? computedResults[index] : null
                                };
                            })
                            .ToImmutableArray();

                    var startIndex = computedResults.OrderBy(y => y.Key).First().Key;
                    result[startIndex].StatusId = (byte)Statuses.NotComputed;

                    return result;
                });
        public static IEnumerable<AnalyzedEntity> GetComparedSample(ILogger<AnalyzerService> logger, in IEnumerable<ReportGetDto> reportsDto, IEnumerable<PriceGetDto>? pricesDto)
        {
            var prices = pricesDto?
                .GroupBy(x => x.Ticker)
                .ToImmutableDictionary(x => x.Key);
            
            return reportsDto
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

                            var firstDate = new DateOnly(report.Year, firstMonth, 1);
                            var lastDate = new DateOnly(report.Year, lastMonth, 28);

                            var price = prices != null && prices.ContainsKey(x.Key)
                                ? prices[x.Key].LastOrDefault(p => p.Date >= firstDate && p.Date <= lastDate)
                                : null;

                            if (price is null)
                                return Array.Empty<Sample>();

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
                    var deviation = computedResults
                        .Where(y => y.Value.HasValue && Math.Abs(y.Value.Value) > 100)
                        .Select(y => y.Key)
                        .ToArray();
                    foreach (var index in deviation)
                        logger.LogWarning(LogEvents.Processing, "Deviation of coefficient > 100%! '{ticker}' at Year: '{year}' Quarter: '{quarter}'", companyOrderedData[index].Ticker, companyOrderedData[index].Year, companyOrderedData[index].Quarter);

                    if (!computedResults.Any())
                        return companyOrderedData
                            .Select(report => new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = QuarterHelper.ToDate(report.Year, report.Quarter),
                                AnalyzedEntityTypeId = (byte) EntityTypes.Coefficient,
                                StatusId = (byte) Statuses.NotComputed
                            });

                    var result = companyOrderedData
                        .Select((report, index) =>
                        {
                            var isComputed = computedResults.ContainsKey(index);

                            return new AnalyzedEntity
                            {
                                CompanyId = x.Key,
                                Date = QuarterHelper.ToDate(report.Year, report.Quarter),
                                AnalyzedEntityTypeId = (byte) EntityTypes.Coefficient,
                                StatusId = isComputed ? (byte) Statuses.Computed : (byte) Statuses.NotComputed,
                                Result = isComputed ? computedResults[index] : null
                            };
                        })
                        .ToImmutableArray();

                    var startIndex = computedResults.OrderBy(y => y.Key).First().Key;
                    result[startIndex].StatusId = (byte) Statuses.NotComputed;

                    return result;
                });
        }
    }
    public static class RatingHelper
    {
        public static Task<Rating> GetRatingAsync(string companyId, IEnumerable<AnalyzedEntity> data) =>
            Task.Run(() =>
            {
                var taskResultPrice = Task.Run(() =>
                {
                    var _data = data.Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Price).ToArray();
                    return _data.Any() ? _data.Sum(x => x.Result) : null;
                });
                var taskResultReport = Task.Run(() =>
                {
                    var _data = data.Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Report).ToArray();
                    return _data.Any() ? _data.Sum(x => x.Result) : null;
                });
                var taskResultCoefficient = Task.Run(() =>
                {
                    var _data = data.Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Coefficient).ToArray();
                    return _data.Any() ? _data.Sum(x => x.Result) : null;
                });

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
        public static Coefficient ComputeCoefficient(ReportGetDto report, PriceGetDto price)
        {
            try
            {
                if (report.Multiplier <= 0 || price.StockVolume is null or <= 0)
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
        public static IDictionary<int, decimal?> ComputeSamplesResults(in Sample[][] samples)
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
        public static decimal? ComputeAverageResult(in decimal?[] sample)
        {
            if (!sample.Any())
                return null;

            var values = sample.Where(x => x is not null).ToArray();

            return values.Length != 0 ? values.Average() : null;
        }
    }
}