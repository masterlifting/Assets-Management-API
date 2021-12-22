using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.Models.Calculator;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.HttpServices.QueryStringBuilder;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class CalculatorCoefficient : IAnalyzerCalculator
{
    private readonly CompanyDataClient client;
    private readonly ILogger<CalculatorCoefficient> logger;
    public CalculatorCoefficient(ILogger<CalculatorCoefficient> logger, CompanyDataClient client)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task<AnalyzedEntity[]> ComputeAsync(IReadOnlyCollection<AnalyzedEntity> data)
    {
        var _data = data
            .Where(x => x.AnalyzedEntityTypeId == (byte)EntityTypes.Coefficient)
            .ToImmutableArray();

        if (!_data.Any())
            return Array.Empty<AnalyzedEntity>();

        var companyIds = _data
            .Select(x => x.CompanyId)
            .Distinct()
            .ToImmutableArray();

        var date = _data.MinBy(x => x.Date)!.Date;
        var quarter = CommonHelper.QarterHelper.GetQuarter(date.Month);

        var reportResponseTask = Task.Run(() => client.Get<ReportGetDto>(
            "reports",
            GetQueryString(HttpRequestFilterType.More, companyIds, date.Year, quarter),
            new(1, int.MaxValue),
            true));
        var priceResponseTask = Task.Run(() => client.Get<PriceGetDto>(
            "prices",
            GetQueryString(HttpRequestFilterType.More, companyIds, date.Year, date.Month, date.Day),
            new(1, int.MaxValue),
            true));

        await Task.WhenAll(reportResponseTask, priceResponseTask);

        if (!reportResponseTask.Result.Errors.Any())
            return GetComparedSample(reportResponseTask.Result.Data!.Items, priceResponseTask.Result.Data?.Items)
                .ToArray();

        foreach (var item in _data)
            item.StatusId = (byte)Statuses.Error;

        return _data.ToArray();
    }
    private IEnumerable<AnalyzedEntity> GetComparedSample(in IEnumerable<ReportGetDto> reportsDto, IReadOnlyCollection<PriceGetDto>? pricesDto) =>
    reportsDto
        .GroupBy(x => x.Ticker)
        .SelectMany(x =>
        {
            var companyOrderedData = x
                .OrderBy(y => y.Year)
                .ThenBy(y => y.Quarter)
                .ToImmutableArray();

            var companyErrorData = new List<AnalyzedEntity>(companyOrderedData.Length);
            var companySamples = new List<Sample[]>(companyOrderedData.Length);

            var id = 0;
            foreach (var report in companyOrderedData)
            {
                try
                {
                    var firstMonth = CommonHelper.QarterHelper.GetFirstMonth(report.Quarter);
                    var lastMonth = CommonHelper.QarterHelper.GetLastMonth(report.Quarter);

                    var firstDate = new DateTime(report.Year, firstMonth, 1);
                    var lastDate = new DateTime(report.Year, lastMonth, 28);

                    var price = pricesDto?
                        .Where(z => z.Date >= firstDate && z.Date <= lastDate)
                        .OrderBy(z => z.Date)
                        .LastOrDefault();

                    var coefficient = ComputeCoefficient(report, price);

                    companySamples.Add(new Sample[]
                    {
                        new() {Id = id, CompareType = CompareTypes.Desc, Value = coefficient.Pe},
                        new() {Id = id, CompareType = CompareTypes.Desc, Value = coefficient.Pb},
                        new() {Id = id, CompareType = CompareTypes.Desc, Value = coefficient.DebtLoad},
                        new() {Id = id, CompareType = CompareTypes.Asc, Value = coefficient.Profitability},
                        new() {Id = id, CompareType = CompareTypes.Asc, Value = coefficient.Roa},
                        new() {Id = id, CompareType = CompareTypes.Asc, Value = coefficient.Roe},
                        new() {Id = id, CompareType = CompareTypes.Asc, Value = coefficient.Eps}
                    });
                }
                catch (Exception exception)
                {
                    companyErrorData.Add(new AnalyzedEntity
                    {
                        CompanyId = x.Key,
                        Date = new DateTime(report.Year, CommonHelper.QarterHelper.GetLastMonth(report.Quarter), 28),
                        AnalyzedEntityTypeId = (byte)EntityTypes.Coefficient,
                        StatusId = (byte)Statuses.Error,
                        Result = 0
                    });

                    logger.LogError(LogEvents.Processing, "{place}. Error: {exception}", nameof(ComputeCoefficient),
                        exception.Message);
                }
                finally
                {
                    id++;
                }
            }

            var comparedSamples = CalculatorService
                .CompareSampleByColumn(companySamples.ToArray(), 7)
                .SelectMany(sample => sample)
                .GroupBy(sample => sample.Id)
                .ToImmutableDictionary(samples => samples.Key);

            return companyErrorData
                .Concat(companyOrderedData
                    .Take(1)
                    .Select(report => new AnalyzedEntity
                    {
                        CompanyId = x.Key,
                        Date = GetDateTime(report.Year, report.Quarter),
                        AnalyzedEntityTypeId = (byte)EntityTypes.Coefficient,
                        StatusId = (byte)Statuses.Starter,
                        Result = 0
                    })
                .Concat(companyOrderedData
                    .Skip(1)
                    .Select((report, index) =>
                    {
                        var isComputed = comparedSamples.ContainsKey(index);

                        return new AnalyzedEntity
                        {
                            CompanyId = x.Key,
                            Date = GetDateTime(report.Year, report.Quarter),
                            AnalyzedEntityTypeId = (byte)EntityTypes.Coefficient,
                            StatusId = isComputed
                                ? (byte)Statuses.Computed
                                : (byte)Statuses.NotComputed,
                            Result = isComputed
                                ? CalculatorService.ComputeSampleResult(comparedSamples[index]
                                    .Select(sample => sample.Value)
                                    .ToImmutableArray())
                                : 0
                        };
                    })));

        });
    private static Coefficient ComputeCoefficient(ReportGetDto report, PriceGetDto? price)
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
    private static DateTime GetDateTime(int year, byte quarter) =>
        new(year, CommonHelper.QarterHelper.GetLastMonth(quarter), 28);
}