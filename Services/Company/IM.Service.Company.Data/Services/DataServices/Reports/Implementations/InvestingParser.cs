using IM.Service.Common.Net;

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.Reports.Interfaces;
using Microsoft.Extensions.Logging;

namespace IM.Service.Company.Data.Services.DataServices.Reports.Implementations
{
    public class InvestingParser : IReportParser
    {
        private readonly ILogger<ReportParser> logger;
        private readonly InvestingParserHandler handler;
        public InvestingParser(ILogger<ReportParser> logger, InvestingClient client)
        {
            this.logger = logger;
            handler = new(client);
        }

        public async Task<Report[]> GetReportsAsync(string source, ReportDataConfigModel config)
        {
            var result = Array.Empty<Report>();

            try
            {
                if (config.SourceValue is null)
                    throw new ArgumentNullException($"source value for '{config.CompanyId}' is null");

                var site = await handler.LoadSiteAsync(config.SourceValue);
                result = InvestingParserHandler.ParseReports(site, config.CompanyId, source);
            }
            catch (Exception exception)
            {
                logger.LogError(LogEvents.Processing, "investing parser error: {error}", exception.InnerException?.Message ?? exception.Message);
            }

            return result;
        }
        public async Task<Report[]> GetReportsAsync(string source, IEnumerable<ReportDataConfigModel> config)
        {
            var _config = config.ToArray();
            var result = new List<Report>(_config.Length * 5);

            foreach (var item in _config)
            {
                result.AddRange(await GetReportsAsync(source, item));
                await Task.Delay(5000);
            }

            return result.ToArray();
        }
    }

    internal class InvestingParserHandler
    {
        private readonly InvestingClient client;
        public InvestingParserHandler(InvestingClient client) => this.client = client;

        internal async Task<HtmlDocument[]> LoadSiteAsync(string sourceValue) =>
            await Task.WhenAll(
                client.GetFinancialPageAsync(sourceValue),
                client.GetBalancePageAsync(sourceValue)
            //client.GetMainPageAsync(sourceValue),
            //client.GetDividendPageAsync(sourceValue)
            );

        internal static Report[] ParseReports(IReadOnlyList<HtmlDocument> site, string companyId, string sourceName)
        {
            var result = new Report[4];
            var culture = CultureInfo.CreateSpecificCulture("ru-RU");

            //var mainPage = new MainPage(site[0]);
            var financialPage = new FinancialPage(site[0], culture);
            var balancePage = new BalancePage(site[1]);
            //var dividendPage = new DividendPage(site[3], financialPage.Dates.ToArray());

            for (var i = 0; i < result.Length; i++)
                result[i] = new()
                {
                    CompanyId = companyId,
                    Year = financialPage.Dates[i].Year,
                    Quarter = CommonHelper.GetQuarter(financialPage.Dates[i].Month),
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
            public FinancialPage(HtmlDocument? page, IFormatProvider culture)
            {
                this.page = page ?? throw new NullReferenceException("Loaded page is null");

                Dates = GetDates(culture);
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
                    if (decimal.TryParse(values[i], out var value))
                        result[i] = value;

                return result;
            }
            private List<DateTime> GetDates(IFormatProvider culture)
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
            public BalancePage(HtmlDocument? page)
            {
                this.page = page ?? throw new NullReferenceException("Loaded page is null");

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
                    if (decimal.TryParse(balanceData[i], out var value))
                        result[i] = value;

                return result;
            }
        }

        #region UnUse
        //private class MainPage
        //{
        //    private readonly HtmlDocument page;
        //    public MainPage(HtmlDocument? page)
        //    {
        //        this.page = page ?? throw new NullReferenceException("Loaded page is null");
        //        StockVolume = GetStockVolume();
        //    }

        //    public long StockVolume { get; }

        //    private long GetStockVolume()
        //    {
        //        var stockVolumeData = page.DocumentNode.SelectNodes("//dt").FirstOrDefault(x => x.InnerText == "Акции в обращении")?.NextSibling?.InnerText;
        //        return stockVolumeData is not null
        //               && long.TryParse(
        //                   stockVolumeData.Replace(".", "")
        //                   , NumberStyles.AllowCurrencySymbol, new CultureInfo("Ru-ru"), out var result)
        //            ? result
        //            : throw new NotSupportedException("Stock volume parsing is  failed");
        //    }
        //}
        //private class DividendPage
        //{
        //    private readonly HtmlDocument page;
        //    public DividendPage(HtmlDocument? page, IReadOnlyList<DateTime> dates)
        //    {
        //        this.page = page ?? throw new NullReferenceException("Loaded page is null");

        //        Dividends = GetDividends(dates);
        //    }
        //    public decimal?[] Dividends { get; }

        //    private decimal?[] GetDividends(IReadOnlyList<DateTime> dates)
        //    {
        //        var result = new decimal?[] { null, null, null, null };

        //        var dividendData = page
        //            .DocumentNode
        //            .SelectNodes("//th[@class]")
        //            .FirstOrDefault(x => x.InnerText == "Экс-дивиденд");

        //        if (dividendData is null)
        //            return result;

        //        var dividendDatesData = dividendData
        //            .ParentNode
        //            .ParentNode
        //            .NextSibling
        //            .NextSibling
        //            .ChildNodes
        //            .Where(x => x.Name == "tr")
        //            .Select(x => x.ChildNodes.Where(y => y.Name == "td")
        //            .Select(z => z.InnerText))
        //            .Select(x => x.FirstOrDefault())
        //            .ToArray();

        //        var dividendDates = new List<DateTime>(dividendDatesData.Length);

        //        foreach (var item in dividendDatesData)
        //            if (DateTime.TryParse(item, out var date))
        //                dividendDates.Add(date);

        //        if (!dividendDates.Any())
        //            return result;

        //        var dividendValueData = dividendData
        //            .ParentNode?
        //            .ParentNode?
        //            .NextSibling?
        //            .NextSibling?
        //            .ChildNodes
        //            .Where(x => x.Name == "tr")
        //            .Select(x => x.ChildNodes.Where(y => y.Name == "td")
        //            .Select(z => z.InnerText))
        //            .Select(x => x.Skip(1)
        //            .FirstOrDefault())
        //            .ToArray();

        //        if (dividendValueData is null)
        //            return result;

        //        for (var i = 0; i < dates.Count; i++)
        //        {
        //            var reportYear = dates[i].Year;
        //            var reportQuarter = CommonHelper.GetQuarter(dates[i].Month);

        //            for (var j = 0; j < dividendDates.Count; j++)
        //            {
        //                var dividendYear = dividendDates[j].Year;
        //                var dividendQuarter = CommonHelper.GetQuarter(dividendDates[j].Month);

        //                if (reportYear != dividendYear || reportQuarter != dividendQuarter)
        //                    continue;

        //                var dividendValue = dividendValueData[j]?.Replace(".", "", StringComparison.CurrentCultureIgnoreCase);

        //                if (!decimal.TryParse(dividendValue, out var config))
        //                    continue;

        //                if (j > 0)
        //                {
        //                    var previousDividendYear = dividendDates[j - 1].Year;
        //                    var previousDividendQuarter = CommonHelper.GetQuarter(dividendDates[j - 1].Month);
        //                    if (dividendYear == previousDividendYear && dividendQuarter == previousDividendQuarter)
        //                    {
        //                        var dividendPreviousValue = dividendValueData[j - 1]?.Replace(".", "", StringComparison.CurrentCultureIgnoreCase);
        //                        if (decimal.TryParse(dividendPreviousValue, out var previousData))
        //                            config += previousData;
        //                    }
        //                }

        //                result[i] = config;
        //            }
        //        }

        //        return result;
        //    }
        //}
        #endregion
    }
}