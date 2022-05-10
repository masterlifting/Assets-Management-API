using System.Collections.Immutable;
using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Models.Services.Calculations.Rating;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;
using static IM.Service.Common.Net.Helpers.LogicHelper;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Entity;

public sealed class RatingComparator
{
    private readonly IServiceScopeFactory scopeFactory;
    public RatingComparator(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task StartAsync()
    {
        var tasks = await Task.WhenAll(
            CompareDataAsync<Price>(),
            CompareDataAsync<Report>(),
            CompareDataAsync<Coefficient>(),
            CompareDataAsync<Dividend>());

        if (tasks.Contains(true))
            await SetComputeRatingTaskAsync();
    }

    private async Task<bool> CompareDataAsync<T>() where T : class, IRating
    {
        var repository = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<T>>();
        var readyData = await repository.GetSampleAsync(x => x.StatusId == (byte)Statuses.Ready || x.StatusId == (byte)Statuses.Error);

        if (!readyData.Any())
            return false;

        try
        {
            await ChangeStatusAsync((byte) Statuses.Computing, readyData, repository);
            var computedData = (await RatingComparatorHandler.GetComparedSampleAsync(repository, readyData)).ToArray();
            await repository.UpdateRangeAsync(computedData, nameof(CompareDataAsync));
            return true;
        }
        catch
        {
            await ChangeStatusAsync((byte)Statuses.Error, readyData, repository);
            return false;
        }
    }
    private static Task ChangeStatusAsync<T>(byte statusId, T[] entities, Repository<T> repository) where T : class, IRating
    {
        foreach (var item in entities)
            item.StatusId = statusId;

        return repository.UpdateRangeAsync(entities, $"Set status '{Enum.Parse<Statuses>(statusId.ToString())}'");
    }

    private Task SetComputeRatingTaskAsync()
    {
        var options = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IOptions<ServiceSettings>>();
        var rabbitConnectionString = options.Value.ConnectionStrings.Mq;

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.Market, QueueEntities.Ratings, QueueActions.Compute, DateTime.UtcNow.ToShortDateString());

        return Task.CompletedTask;
    }
}
internal class RatingComparatorHandler
{
    internal static async Task<IEnumerable<T>> GetComparedSampleAsync<T>(Repository<T> repository, IEnumerable<T> readyData) where T : class, IRating =>
        readyData switch
        {
            IEnumerable<Price> => (IEnumerable<T>)await GetComparedSampleAsync(repository as Repository<Price>, readyData.ToArray() as Price[]),
            IEnumerable<Dividend> => (IEnumerable<T>)await GetComparedSampleAsync(repository as Repository<Dividend>, readyData.ToArray() as Dividend[]),
            IEnumerable<Report> => (IEnumerable<T>)await GetComparedSampleAsync(repository as Repository<Report>, readyData.ToArray() as Report[]),
            IEnumerable<Coefficient> => (IEnumerable<T>)await GetComparedSampleAsync(repository as Repository<Coefficient>, readyData.ToArray() as Coefficient[]),
            _ => throw new ArgumentOutOfRangeException(nameof(readyData),"Не удалось определить объект для расчетов данных для рейтинга")
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

                var computedResults = RatingCalculatorHelper
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

                var computedResults = RatingCalculatorHelper
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
        var (year, quarter) = QuarterHelper.SubtractQuarter(reportMin.Year, reportMin.Quarter);
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

                var computedResults = RatingCalculatorHelper.ComputeSamplesResults(companySamples);

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
        var (year, quarter) = QuarterHelper.SubtractQuarter(coefficientMin.Year, coefficientMin.Quarter);
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

                var computedResults = RatingCalculatorHelper.ComputeSamplesResults(companySamples);

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
