using HtmlAgilityPack;

using IM.Service.Common.Net;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IM.Service.Company.Data.Models.Data;

namespace IM.Service.Company.Data.Services.DataServices.Reports.Implementations;

public class InvestingGrabber : IDataGrabber
{
    private readonly Repository<Report> repository;
    private readonly ILogger<ReportGrabber> logger;
    private readonly InvestingParserHandler handler;

    public InvestingGrabber(Repository<Report> repository, ILogger<ReportGrabber> logger, InvestingClient client)
    {
        this.repository = repository;
        this.logger = logger;
        handler = new(client);
    }

    public async Task GrabCurrentDataAsync(string source, DataConfigModel config) => 
        await GrabHistoryDataAsync(source, config);
    public async Task GrabCurrentDataAsync(string source, IEnumerable<DataConfigModel> configs) => 
        await GrabHistoryDataAsync(source, configs);

    public async Task GrabHistoryDataAsync(string source, DataConfigModel config)
    {
        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException(config.CompanyId);

            var data = await handler.LoadSiteAsync(config.SourceValue);

            var result = InvestingParserHandler.Parse(data, config.CompanyId, source);

            await repository.CreateUpdateAsync(result, new CompanyQuarterComparer<Report>(), "Investing history reports");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(InvestingGrabber) + '.' + nameof(GrabCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GrabHistoryDataAsync(string source, IEnumerable<DataConfigModel> configs)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

        foreach (var config in configs)
            if (await timer.WaitForNextTickAsync())
                await GrabHistoryDataAsync(source, config);
    }
}

internal sealed class InvestingParserHandler
{
    private readonly InvestingClient client;
    public InvestingParserHandler(InvestingClient client) => this.client = client;

    internal async Task<HtmlDocument[]> LoadSiteAsync(string sourceValue) =>
        await Task.WhenAll(
            client.GetFinancialPageAsync(sourceValue),
            client.GetBalancePageAsync(sourceValue)
        );

    internal static IEnumerable<Report> Parse(IReadOnlyList<HtmlDocument> site, string companyId, string sourceName)
    {
        var result = new Report[4];
        var culture = CultureInfo.CreateSpecificCulture("ru-RU");

        var financialPage = new FinancialPage(site[0], culture);
        var balancePage = new BalancePage(site[1], culture);

        for (var i = 0; i < result.Length; i++)
            result[i] = new()
            {
                CompanyId = companyId,
                Year = financialPage.Dates[i].Year,
                Quarter = CommonHelper.QuarterHelper.GetQuarter(financialPage.Dates[i].Month),
                SourceType = sourceName,
                Multiplier = 1_000_000,

                Turnover = balancePage.Turnovers[i],
                LongTermDebt = balancePage.LongDebts[i],
                Asset = financialPage.Assets[i],
                CashFlow = financialPage.CashFlows[i],
                Obligation = financialPage.Obligations[i],
                ProfitGross = financialPage.ProfitGross[i],
                ProfitNet = financialPage.ProfitNet[i],
                Revenue = financialPage.Revenues[i],
                ShareCapital = financialPage.ShareCapitals[i],
            };

        return result;
    }

    private class FinancialPage
    {
        private readonly HtmlDocument page;
        private readonly IFormatProvider culture;

        public FinancialPage(HtmlDocument? page, IFormatProvider culture)
        {
            this.page = page ?? throw new NullReferenceException("Loaded page is null");
            this.culture = culture;

            Dates = GetDates();
            Revenues = GetFinancialData(0, "Общий доход");
            ProfitGross = GetFinancialData(0, "Валовая прибыль");
            ProfitNet = GetFinancialData(0, "Чистая прибыль");
            Assets = GetFinancialData(1, "Итого активы");
            Obligations = GetFinancialData(1, "Итого обязательства");
            ShareCapitals = GetFinancialData(1, "Итого акционерный капитал");
            CashFlows = GetFinancialData(2, "Чистое изменение денежных средств");
        }

        public List<DateTime> Dates { get; }
        public decimal?[] Revenues { get; }
        public decimal?[] ProfitGross { get; }
        public decimal?[] ProfitNet { get; }
        public decimal?[] Assets { get; }
        public decimal?[] Obligations { get; }
        public decimal?[] ShareCapitals { get; }
        public decimal?[] CashFlows { get; }

        private decimal?[] GetFinancialData(int tableIndex, string pattern)
        {
            var error = $"Financial config for '{pattern}' is null!";
            var result = new decimal?[] { null, null, null, null };

            var prepareData = page
                .DocumentNode
                .SelectNodes("//table[@class='genTbl openTbl companyFinancialSummaryTbl']/tbody")[tableIndex]
                ?.ChildNodes;

            if (prepareData is null)
                throw new NotSupportedException(error);

            var financialData = prepareData.FirstOrDefault(x => x.Name == "tr" && x.InnerText.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase) > 0);

            if (financialData is null)
                return result;

            var values = financialData
                .ChildNodes
                .Where(x => x.Name == "td")
                .Skip(1)
                .Select(x => x.InnerText)
                .ToArray();

            if (values is null)
                throw new NotSupportedException(error);

            for (var i = 0; i < values.Length; i++)
                if (decimal.TryParse(values[i], NumberStyles.Currency, culture, out var value))
                    result[i] = value;

            return result;
        }
        private List<DateTime> GetDates()
        {
            var result = new List<DateTime>(4);
            const string? error = "Financial page date parsing failed!";

            var dateNode = page.DocumentNode.SelectNodes("//th[@class='arial_11 noBold title right period']").FirstOrDefault();

            if (dateNode is null)
                throw new NotSupportedException(error);

            var dates = dateNode.ParentNode.InnerText.Split("\n");

            foreach (var d in dates)
                if (DateTime.TryParse(d, culture, DateTimeStyles.AssumeLocal, out var date))
                    result.Add(date);

            return result.Count < 4 ? throw new NotSupportedException(error) : result;
        }
    }
    private class BalancePage
    {
        private readonly HtmlDocument page;
        private readonly IFormatProvider culture;

        public BalancePage(HtmlDocument? page, IFormatProvider culture)
        {
            this.page = page ?? throw new NullReferenceException("Loaded page is null");
            this.culture = culture;

            Turnovers = GetBalanceData("Итого оборотные активы");
            LongDebts = GetBalanceData("Общая долгосрочная задолженность по кредитам и займам");
        }

        public decimal?[] Turnovers { get; }
        public decimal?[] LongDebts { get; }

        private decimal?[] GetBalanceData(string pattern)
        {
            var error = $"Financial config for '{pattern}' is null!";
            var result = new decimal?[] { null, null, null, null };

            var prepareData = page.DocumentNode
                .SelectNodes("//span[@class]")
                .FirstOrDefault(x => x.InnerText == pattern);

            if (prepareData is null)
                return result;

            var balanceData = prepareData
                .ParentNode?.ParentNode?.ChildNodes
                .Where(x => x.Name == "td")
                .Skip(1)
                .Select(x => x.InnerText)
                .ToArray();

            if (balanceData is null)
                throw new NotSupportedException(error);

            for (var i = 0; i < balanceData.Length; i++)
                if (decimal.TryParse(balanceData[i], NumberStyles.Currency, culture, out var value))
                    result[i] = value;

            return result;
        }
    }
}