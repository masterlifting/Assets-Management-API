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
        var culture = new CultureInfo("ru-RU");

        var financialPage = new FinancialPage(site[0], culture);
        var balancePage = new BalancePage(site[1], culture, financialPage.ReportsCount);

        var result = new List<Report>(financialPage.ReportsCount);

        for (var i = 0; i < financialPage.ReportsCount; i++)
            result.Add(new()
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
            });

        return result;
    }

    private class FinancialPage
    {
        private readonly HtmlDocument page;
        private readonly IFormatProvider culture;
        public int ReportsCount { get; }

        public FinancialPage(HtmlDocument? page, IFormatProvider culture)
        {
            this.page = page ?? throw new NullReferenceException("Loaded page is null");
            this.culture = culture;

            Dates = GetDates();
            ReportsCount = Dates.Count;

            if (ReportsCount == 0)
                throw new NotSupportedException($"{nameof(FinancialPage)}. Error: Reports not found!");

            Revenues = GetData(0, "Общий доход");
            ProfitGross = GetData(0, "Валовая прибыль");
            ProfitNet = GetData(0, "Чистая прибыль");
            Assets = GetData(1, "Итого активы");
            Obligations = GetData(1, "Итого обязательства");
            ShareCapitals = GetData(1, "Итого акционерный капитал");
            CashFlows = GetData(2, "Чистое изменение денежных средств");
        }

        public List<DateTime> Dates { get; }
        public decimal?[] Revenues { get; }
        public decimal?[] ProfitGross { get; }
        public decimal?[] ProfitNet { get; }
        public decimal?[] Assets { get; }
        public decimal?[] Obligations { get; }
        public decimal?[] ShareCapitals { get; }
        public decimal?[] CashFlows { get; }

        private decimal?[] GetData(int tableIndex, string pattern)
        {
            var result = new List<decimal?>(ReportsCount);

            var prepareData = page
                .DocumentNode
                .SelectNodes("//table[@class='genTbl openTbl companyFinancialSummaryTbl']/tbody")[tableIndex]
                ?.ChildNodes;

            if (prepareData is null)
                throw new NotSupportedException($"{nameof(GetData)}. Error: {nameof(HtmlNodeCollection)} is null");

            var data = prepareData.FirstOrDefault(x => x.Name == "tr" && x.InnerText.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase) > 0);

            if (data is null)
            {
                for (var i = 0; i < ReportsCount; i++)
                    result.Add(null);

                return result.ToArray();
            }

            var values = data
                .ChildNodes
                .Where(x => x.Name == "td")
                .Skip(1)
                .Select(x => x.InnerText)
                .ToArray();

            if (values is null)
                throw new NotSupportedException($"{nameof(GetData)}. Error: {nameof(values)} is null");

            foreach (var item in values)
                if (decimal.TryParse(item, NumberStyles.Currency, culture, out var value))
                    result.Add(value);
                else
                    result.Add(null);

            return result.ToArray();
        }
        private List<DateTime> GetDates()
        {
            var result = new List<DateTime>(4);

            var dateNode = page.DocumentNode.SelectNodes("//th[@class='arial_11 noBold title right period']").FirstOrDefault();

            if (dateNode is null)
                throw new NotSupportedException($"{nameof(GetDates)}. Error: {nameof(HtmlNode)} is null");

            var dates = dateNode.ParentNode.InnerText.Split("\n");

            foreach (var item in dates)
                if (DateTime.TryParse(item, culture, DateTimeStyles.AssumeLocal, out var date))
                    result.Add(date);

            return result;
        }
    }
    private class BalancePage
    {
        private readonly HtmlDocument page;
        private readonly IFormatProvider culture;
        private readonly int reportsCount;

        public BalancePage(HtmlDocument? page, IFormatProvider culture, int reportsCount)
        {
            this.page = page ?? throw new NullReferenceException("Loaded page is null");
            this.culture = culture;
            this.reportsCount = reportsCount;

            Turnovers = GetData("Итого оборотные активы");
            LongDebts = GetData("Общая долгосрочная задолженность по кредитам и займам");
        }

        public decimal?[] Turnovers { get; }
        public decimal?[] LongDebts { get; }

        private decimal?[] GetData(string pattern)
        {
            var result = new List<decimal?>(reportsCount);

            var prepareData = page.DocumentNode
                .SelectNodes("//span[@class]")
                .FirstOrDefault(x => x.InnerText == pattern);

            if (prepareData is null)
            {
                for (var i = 0; i < reportsCount; i++)
                    result.Add(null);

                return result.ToArray();
            }

            var data = prepareData
                .ParentNode?.ParentNode?.ChildNodes
                .Where(x => x.Name == "td")
                .Skip(1)
                .Select(x => x.InnerText)
                .ToArray();

            if (data is null)
                throw new NotSupportedException($"{nameof(GetData)}. Error: {nameof(HtmlNode)} is null");

            foreach (var item in data)
                if (decimal.TryParse(item, NumberStyles.Currency, culture, out var value))
                    result.Add(value);
                else
                    result.Add(null);

            return result.ToArray();
        }
    }
}