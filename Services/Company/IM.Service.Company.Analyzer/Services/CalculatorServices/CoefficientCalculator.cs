using System;
using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Company.Analyzer.Models.Calculator;
using Microsoft.Extensions.DependencyInjection;
using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.HttpServices.QueryStringBuilder;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;


public class CoefficientCalculator : IAnalyzerCalculator
{
    private readonly CompanyDataClient reportClient;
    private readonly CompanyDataClient priceClient;
    public CoefficientCalculator(IServiceScopeFactory scopeFactory)
    {
        reportClient = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<CompanyDataClient>();
        priceClient = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<CompanyDataClient>();
    }

    public async Task<RatingData[]> ComputeAsync(IEnumerable<AnalyzedEntity> data)
    {
        var companies = data.GroupBy(x => x.CompanyId).ToDictionary(x => x.Key);
        List<Task<ResponseModel<PaginatedModel<ReportGetDto>>>> reportTasks = new(companies.Count);
        List<Task<ResponseModel<PaginatedModel<PriceGetDto>>>> priceTasks = new(companies.Count);

        foreach (var (key, entities) in companies)
        {
            var date = entities.OrderBy(x => x.Date).First().Date;
            var quarter = CommonHelper.QarterHelper.GetQuarter(date.Month);

            reportTasks.Add(reportClient.Get<ReportGetDto>(
                "reports",
                GetQueryString(HttpRequestFilterType.More, key, date.Year, quarter),
                new(1, int.MaxValue)));

            priceTasks.Add(priceClient.Get<PriceGetDto>(
                "prices",
                GetQueryString(HttpRequestFilterType.More, key, date.Year, date.Month, date.Day),
                new(1, int.MaxValue)));
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
    private static IEnumerable<RatingData> GetComparedSample(IEnumerable<ReportGetDto> reportsDto, IEnumerable<PriceGetDto>? pricesDto) =>
    reportsDto
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Quarter)
        .Select(x =>
        {
            var month = CommonHelper.QarterHelper.GetLastMonth(x.Quarter);
            var date = new DateTime(x.Year, month, 1);
            var price = pricesDto?.FirstOrDefault(y => y.Date.Date >= date.Date);

            if (price is null)
                return Array.Empty<Sample>();

            var coefficient = ComputeCoefficient(x, price);

            return new Sample[]
            {
                    new() {CompanyId = x.Ticker, CompareTypes = Enums.CompareTypes.Desc, Value = coefficient.Pe},
                    new() {CompanyId = x.Ticker, CompareTypes = Enums.CompareTypes.Desc, Value = coefficient.Pb},
                    new() {CompanyId = x.Ticker, CompareTypes = Enums.CompareTypes.Desc, Value = coefficient.DebtLoad},
                    new() {CompanyId = x.Ticker, CompareTypes = Enums.CompareTypes.Asc, Value = coefficient.Profitability},
                    new() {CompanyId = x.Ticker, CompareTypes = Enums.CompareTypes.Asc, Value = coefficient.Roa},
                    new() {CompanyId = x.Ticker, CompareTypes = Enums.CompareTypes.Asc, Value = coefficient.Roe},
                    new() {CompanyId = x.Ticker, CompareTypes = Enums.CompareTypes.Asc, Value = coefficient.Eps}
            };
        })
        .Select(RatingComparator.CompareSample)
        .Select((x, i) => new RatingData
        {
            CompanyId = x[i].CompanyId,
            AnalyzedEntityTypeId = (byte)Enums.EntityTypes.Coefficient,
            Result = RatingComparator.ComputeSampleResult(x.Select(y => y.Value).ToArray())
        });
    private static Coefficient ComputeCoefficient(ReportGetDto report, PriceGetDto price)
    {
        if (report.Multiplier <= 0 || price.StockVolume is null or <= 0)
            throw new ArgumentException($"{nameof(ComputeCoefficient)} parameters is incorrect");

        var profitNet = report.ProfitNet ?? throw new ArgumentNullException($"{nameof(report.ProfitNet)} is null!");
        var revenue = report.Revenue ?? throw new ArgumentNullException($"{nameof(report.Revenue)} is null!");
        var asset = report.Asset ?? throw new ArgumentNullException($"{nameof(report.Asset)} is null!");
        var shareCapital = report.ShareCapital ?? throw new ArgumentNullException($"{nameof(report.ShareCapital)} is null!");
        var obligation = report.Obligation ?? throw new ArgumentNullException($"{nameof(report.Obligation)} is null!");

        try
        {
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
        catch
        {
            throw new ArithmeticException("Coefficient calculating error!");
        }
    }
}