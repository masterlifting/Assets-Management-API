using IM.Service.Shared.Helpers;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Models.Services.Calculations.Rating;

using System.Collections.Immutable;

using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Helpers;

internal static class RatingHelper
{
    internal static class RatingComparator
    {
        internal static async Task<IEnumerable<T>> GetComparedSampleAsync<T>(Repository<T> repository, IEnumerable<T> readyData) where T : class, IRating =>
            readyData switch
            {
                IEnumerable<Price> => (IEnumerable<T>)await GetComparedSampleAsync(repository as Repository<Price>, readyData.ToArray() as Price[]),
                IEnumerable<Dividend> => (IEnumerable<T>)await GetComparedSampleAsync(repository as Repository<Dividend>, readyData.ToArray() as Dividend[]),
                IEnumerable<Report> => (IEnumerable<T>)await GetComparedSampleAsync(repository as Repository<Report>, readyData.ToArray() as Report[]),
                IEnumerable<Coefficient> => (IEnumerable<T>)await GetComparedSampleAsync(repository as Repository<Coefficient>, readyData.ToArray() as Coefficient[]),
                _ => throw new ArgumentOutOfRangeException(nameof(readyData), "Не удалось определить объект для расчетов данных для рейтинга")
            };

        private static async Task<IEnumerable<Price>> GetComparedSampleAsync(Repository<Price>? repository, Price[]? readyData)
        {
            if (repository is null) throw new ArgumentNullException(nameof(repository));
            if (readyData is null) throw new ArgumentNullException(nameof(readyData));

            var dateMin = readyData.Min(x => x.Date).AddDays(-30);
            var companyIds = readyData.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
            var sourceIds = readyData.GroupBy(x => x.SourceId).Select(x => x.Key).ToArray();

            var data = await repository.GetSampleAsync(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId)
                && x.Date >= dateMin);

            return data
                .GroupBy(x => (x.CompanyId, x.SourceId))
                .SelectMany(x =>
                {
                    var companyOrderedData = x
                        .OrderBy(y => y.Date)
                        .ToImmutableArray();

                    var companySample = companyOrderedData
                        .Select((price, index) => new Sample
                        { Id = index, CompareType = CompareTypes.Asc, Value = price.ValueTrue })
                        .ToArray();

                    var computedResults = RatingComputer
                        .ComputeSampleResults(companySample)
                        .ToImmutableDictionary(y => y.Id, z => z.Value);

                //check deviation
                //var deviation = computedResults
                //    .Where(y => y.Value.HasValue && Math.Abs(y.Value.Value) > 50)
                //    .Select(y => y.Key)
                //    .ToArray();
                //foreach (var index in deviation)
                //    logger.LogWarning(LogEvents.Processing, "Deviation of price > 50%! '{ticker}' at '{date}'",
                //        companyOrderedData[index].CompanyId, companyOrderedData[index].Date.ToShortDateString());

                    return !computedResults.Any()
                        ? companyOrderedData
                            .Select(price =>
                            {
                                price.StatusId = (byte)Statuses.Computed;
                                return price;
                            })
                        : companyOrderedData
                            .Select((price, index) =>
                            {
                                if (index == 0)
                                    price.StatusId = (byte)Statuses.Computed;
                                else
                                {
                                    var isComputed = computedResults.ContainsKey(index);
                                    price.StatusId = isComputed ? (byte)Statuses.Computed : (byte)Statuses.NotComputed;
                                    price.Result = isComputed ? computedResults[index] : null;
                                }

                                return price;
                            });
                });
        }
        private static async Task<IEnumerable<Dividend>> GetComparedSampleAsync(Repository<Dividend>? repository, Dividend[]? readyData)
        {
            if (repository is null) throw new ArgumentNullException(nameof(repository));
            if (readyData is null) throw new ArgumentNullException(nameof(readyData));

            var dateMin = readyData.Min(x => x.Date).AddDays(-30);
            var companyIds = readyData.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
            var sourceIds = readyData.GroupBy(x => x.SourceId).Select(x => x.Key).ToArray();

            var data = await repository.GetSampleAsync(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId)
                && x.Date >= dateMin);

            return data
                .GroupBy(x => (x.CompanyId, x.SourceId))
                .SelectMany(x =>
                {
                    var companyOrderedData = x
                        .OrderBy(y => y.Date)
                        .ToImmutableArray();

                    var companySample = companyOrderedData
                        .Select((dividend, index) => new Sample
                        { Id = index, CompareType = CompareTypes.Asc, Value = dividend.Value })
                        .ToArray();

                    var computedResults = RatingComputer
                        .ComputeSampleResults(companySample)
                        .ToImmutableDictionary(y => y.Id, z => z.Value);

                //check deviation
                //var deviation = computedResults
                //    .Where(y => y.Value.HasValue && Math.Abs(y.Value.Value) > 50)
                //    .Select(y => y.Key)
                //    .ToArray();
                //foreach (var index in deviation)
                //    logger.LogWarning(LogEvents.Processing, "Deviation of dividend > 50%! '{ticker}' at '{date}'",
                //        companyOrderedData[index].CompanyId, companyOrderedData[index].Date.ToShortDateString());

                    return !computedResults.Any()
                        ? companyOrderedData
                            .Select(dividend =>
                            {
                                dividend.StatusId = (byte)Statuses.Computed;
                                return dividend;
                            })
                        : companyOrderedData
                            .Select((dividend, index) =>
                            {
                                if (index == 0)
                                    dividend.SourceId = (byte)Statuses.Computed;
                                else
                                {
                                    var isComputed = computedResults.ContainsKey(index);
                                    dividend.StatusId = isComputed ? (byte)Statuses.Computed : (byte)Statuses.NotComputed;
                                    dividend.Result = isComputed ? computedResults[index] : null;
                                }

                                return dividend;
                            });
                });
        }
        private static async Task<IEnumerable<Report>> GetComparedSampleAsync(Repository<Report>? repository, Report[]? readyData)
        {
            if (repository is null) throw new ArgumentNullException(nameof(repository));
            if (readyData is null) throw new ArgumentNullException(nameof(readyData));

            var reportMin = readyData.Min(x => (x.Year, x.Quarter));
            var (year, quarter) = LogicHelper.QuarterHelper.SubtractQuarter(reportMin.Year, reportMin.Quarter);
            var companyIds = readyData.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
            var sourceIds = readyData.GroupBy(x => x.SourceId).Select(x => x.Key).ToArray();

            var data = await repository.GetSampleAsync(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId)
                && x.Year > year
                || x.Year == year && x.Quarter >= quarter);

            return data
                .GroupBy(x => (x.CompanyId, x.SourceId))
                .SelectMany(x =>
                {
                    var companyOrderedData = x
                        .OrderBy(y => y.Year)
                        .ThenBy(y => y.Quarter)
                        .ToImmutableArray();

                    var companySamples = companyOrderedData
                        .Select((report, index) => new Sample[]
                        {
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = report.Revenue},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = report.ProfitNet},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = report.ProfitGross},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = report.Asset},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = report.Turnover},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = report.ShareCapital},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = report.CashFlow},
                        new() {Id = index, CompareType = CompareTypes.Desc, Value = report.Obligation},
                        new() {Id = index, CompareType = CompareTypes.Desc, Value = report.LongTermDebt}
                        })
                        .ToArray();

                    var computedResults = RatingComputer.ComputeSamplesResults(companySamples);

                //check deviation
                //var deviation = computedResults
                //    .Where(y => y.Value.HasValue && Math.Abs(y.Value.Value) > 100)
                //    .Select(y => y.Key)
                //    .ToArray();
                //foreach (var index in deviation)
                //    logger.LogWarning(LogEvents.Processing,
                //        "Deviation of report > 100%! '{ticker}' at Year: '{year}' Quarter: '{quarter}'",
                //        companyOrderedData[index].CompanyId, companyOrderedData[index].Year,
                //        companyOrderedData[index].Quarter);

                    return !computedResults.Any()
                        ? companyOrderedData
                            .Select(report =>
                            {
                                report.StatusId = (byte)Statuses.Computed;
                                return report;
                            })
                        : companyOrderedData
                            .Select((report, index) =>
                            {
                                if (index == 0)
                                    report.StatusId = (byte)Statuses.Computed;
                                else
                                {
                                    var isComputed = computedResults.ContainsKey(index);
                                    report.StatusId = isComputed ? (byte)Statuses.Computed : (byte)Statuses.NotComputed;
                                    report.Result = isComputed ? computedResults[index] : null;
                                }

                                return report;
                            });
                });
        }
        private static async Task<IEnumerable<Coefficient>> GetComparedSampleAsync(Repository<Coefficient>? repository, Coefficient[]? readyData)
        {
            if (repository is null) throw new ArgumentNullException(nameof(repository));
            if (readyData is null) throw new ArgumentNullException(nameof(readyData));

            var coefficientMin = readyData.Min(x => (x.Year, x.Quarter));
            var (year, quarter) = LogicHelper.QuarterHelper.SubtractQuarter(coefficientMin.Year, coefficientMin.Quarter);
            var companyIds = readyData.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
            var sourceIds = readyData.GroupBy(x => x.SourceId).Select(x => x.Key).ToArray();

            var data = await repository.GetSampleAsync(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId)
                && x.Year > year
                || x.Year == year && x.Quarter >= quarter);

            return data
                .GroupBy(x => (x.CompanyId, x.SourceId))
                .SelectMany(x =>
                {
                    var companyOrderedData = x
                        .OrderBy(y => y.Year)
                        .ThenBy(y => y.Quarter)
                        .ToImmutableArray();

                    var companySamples = companyOrderedData
                        .Select((coefficient, index) => new Sample[]
                        {
                        new() {Id = index, CompareType = CompareTypes.Desc, Value = coefficient.Pe},
                        new() {Id = index, CompareType = CompareTypes.Desc, Value = coefficient.Pb},
                        new() {Id = index, CompareType = CompareTypes.Desc, Value = coefficient.DebtLoad},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = coefficient.Profitability},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = coefficient.Roa},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = coefficient.Roe},
                        new() {Id = index, CompareType = CompareTypes.Asc, Value = coefficient.Eps}
                        })
                        .ToArray();

                    var computedResults = RatingComputer.ComputeSamplesResults(companySamples);

                //check deviation
                //var deviation = computedResults
                //    .Where(y => y.Value.HasValue && Math.Abs(y.Value.Value) > 100)
                //    .Select(y => y.Key)
                //    .ToArray();
                //foreach (var index in deviation)
                //    logger.LogWarning(LogEvents.Processing,
                //        "Deviation of coefficient > 100%! '{ticker}' at Year: '{year}' Quarter: '{quarter}'",
                //        companyOrderedData[index].CompanyId, companyOrderedData[index].Year,
                //        companyOrderedData[index].Quarter);

                    return !computedResults.Any()
                        ? companyOrderedData
                            .Select(coefficient =>
                            {
                                coefficient.StatusId = (byte)Statuses.Computed;
                                return coefficient;
                            })
                        : companyOrderedData
                            .Select((coefficient, index) =>
                            {
                                if (index == 0)
                                    coefficient.StatusId = (byte)Statuses.Computed;
                                else
                                {
                                    var isComputed = computedResults.ContainsKey(index);
                                    coefficient.StatusId = isComputed ? (byte)Statuses.Computed : (byte)Statuses.NotComputed;
                                    coefficient.Result = isComputed ? computedResults[index] : null;
                                }

                                return coefficient;
                            });
                });
        }
    }
    internal static class RatingComputer
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
}