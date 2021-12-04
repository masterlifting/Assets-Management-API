using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.Models.Calculator;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
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

    public async Task<AnalyzedEntity[]> ComputeAsync(IEnumerable<AnalyzedEntity> data)
    {
        var companies = data
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key);

        List<Task<ResponseModel<PaginatedModel<ReportGetDto>>>> reportTasks = new(companies.Count);
        List<Task<ResponseModel<PaginatedModel<PriceGetDto>>>> priceTasks = new(companies.Count);

        foreach (var (key, group) in companies)
        {
            var date = group.OrderBy(x => x.Date).First().Date;
            
            var quarter = CommonHelper.QarterHelper.GetQuarter(date.Month);
            reportTasks.Add(client.Get<ReportGetDto>(
                "reports",
                GetQueryString(HttpRequestFilterType.More, key, date.Year, quarter),
                new(1, int.MaxValue),
                true));

            priceTasks.Add(client.Get<PriceGetDto>(
                "prices",
                GetQueryString(HttpRequestFilterType.More, key, date.Year, date.Month, date.Day),
                new(1, int.MaxValue),
                true));
        }

        var reportsResults = Task.WhenAll(reportTasks);
        var pricesResults = Task.WhenAll(priceTasks);

        await Task.WhenAll(reportsResults, pricesResults);

        var prices = pricesResults.Result
            .Where(x => !x.Errors.Any())
            .SelectMany(x => x.Data!.Items);

        return reportsResults.Result
            .Where(x => !x.Errors.Any())
            .SelectMany(x => GetComparedSample(x.Data!.Items, prices))
            .ToArray();
    }
    private IEnumerable<AnalyzedEntity> GetComparedSample(IEnumerable<ReportGetDto> reportsDto, IEnumerable<PriceGetDto>? pricesDto) =>
    reportsDto
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Quarter)
        .Select(x =>
        {
            var month = CommonHelper.QarterHelper.GetLastMonth(x.Quarter);
            var date = new DateTime(x.Year, month, 1);
            var price = pricesDto?.FirstOrDefault(y => y.Date.Date >= date.Date);

            return price is not null && TryComputeCoefficient(x, price, out var coefficient)
                ? new Sample[]
                {
                    new() {CompanyId = x.Ticker, Date = date, CompareTypes = CompareTypes.Desc, Value = coefficient!.Pe},
                    new() {CompanyId = x.Ticker, Date = date, CompareTypes = CompareTypes.Desc, Value = coefficient.Pb},
                    new() {CompanyId = x.Ticker, Date = date, CompareTypes = CompareTypes.Desc, Value = coefficient.DebtLoad},
                    new() {CompanyId = x.Ticker, Date = date, CompareTypes = CompareTypes.Asc, Value = coefficient.Profitability},
                    new() {CompanyId = x.Ticker, Date = date, CompareTypes = CompareTypes.Asc, Value = coefficient.Roa},
                    new() {CompanyId = x.Ticker, Date = date, CompareTypes = CompareTypes.Asc, Value = coefficient.Roe},
                    new() {CompanyId = x.Ticker, Date = date, CompareTypes = CompareTypes.Asc, Value = coefficient.Eps}
                }
                : Array.Empty<Sample>();
        })
        .Select(CalculatorService.CompareSample)
        .Select(x => new AnalyzedEntity
        {
            CompanyId = x[0].CompanyId,
            Date = x[0].Date,
            AnalyzedEntityTypeId = (byte)EntityTypes.Coefficient,
            Result = CalculatorService.ComputeSampleResult(x.Select(y => y.Value))
        });
    private bool TryComputeCoefficient(ReportGetDto report, PriceGetDto price, out Coefficient? coefficient)
    {
        coefficient = null;

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

            coefficient = new()
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
            logger.LogError(LogEvents.Processing, "{place}. Error: {exception}", nameof(TryComputeCoefficient), exception.Message);
            return false;
        }

        return true;
    }
}