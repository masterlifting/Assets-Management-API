using CommonServices;

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Reports.Clients;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.Services.ReportServices.Interfaces;

namespace IM.Service.Company.Reports.Services.ReportServices.Implementations
{
    public class InvestingParser : IReportParser
    {
        private readonly InvestingClient client;
        public InvestingParser(InvestingClient client) => this.client = client;

        public async Task<Report[]> GetReportsAsync(Ticker ticker)
        {
            Report[] result = Array.Empty<Report>();

            if (ticker.SourceValue is null)
            {
                Console.WriteLine($"report source for '{ticker.Name}' not found!");
                return result;
            }

            try
            {
                var pages = await LoadSiteAsync(ticker.SourceValue);
                result = GetReports(pages, ticker.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                return result;
            }

            return result;
        }

        private async Task<HtmlDocument[]> LoadSiteAsync(string sourceValue) =>
            await Task.WhenAll(
            client.GetMainPageAsync(sourceValue),
            client.GetFinancialPageAsync(sourceValue),
            client.GetBalancePageAsync(sourceValue),
            client.GetDividendPageAsync(sourceValue));
        private static Report[] GetReports(IReadOnlyList<HtmlDocument> site, string tickerName)
        {
            var result = new Report[4];
            var culture = CultureInfo.CreateSpecificCulture("ru-RU");

            var mainPage = new MainPage(site[0]);
            var financialPage = new FinancialPage(site[1], culture);
            var balancePage = new BalancePage(site[2]);
            var dividendPage = new DividendPage(site[3], financialPage.Dates.ToArray());

            for (var i = 0; i < 4; i++)
                result[i] = new()
                {
                    TickerName = tickerName,
                    Year = financialPage.Dates[i].Year,
                    Quarter = CommonHelper.GetQuarter(financialPage.Dates[i].Month),
                    StockVolume = mainPage.StockVolume,
                    Turnover = balancePage.Turnovers[i],
                    LongTermDebt = balancePage.LongDebts[i],
                    Asset = financialPage.Assets[i],
                    CashFlow = financialPage.CashFlows[i],
                    Obligation = financialPage.Obligations[i],
                    ProfitGross = financialPage.ProfitsGross[i],
                    ProfitNet = financialPage.ProfitsNet[i],
                    Revenue = financialPage.Revenues[i],
                    ShareCapital = financialPage.ShareCapitals[i],
                    Dividend = dividendPage.Dividends[i]
                };

            return result.ToArray();
        }
    }

    internal class MainPage
    {
        private readonly HtmlDocument page;
        public MainPage(HtmlDocument? page)
        {
            this.page = page ?? throw new NullReferenceException("loaded page is null");
            StockVolume = GetStockVolume();
        }

        public long StockVolume { get; }

        private long GetStockVolume()
        {
            var stockVolumeData = page.DocumentNode.SelectNodes("//dt").FirstOrDefault(x => x.InnerText == "Акции в обращении")?.NextSibling?.InnerText;
            return stockVolumeData is not null && long.TryParse(stockVolumeData.Replace(".", ""), out var result)
                ? result
                : throw new NotSupportedException("parsing is stock volume failed");
        }
    }
    internal class FinancialPage
    {
        private readonly HtmlDocument page;
        public FinancialPage(HtmlDocument? page, IFormatProvider culture)
        {
            this.page = page ?? throw new NullReferenceException("loaded page is null");

            Dates = GetDates(culture);
            Revenues = GetFinancialData(0, "Общий доход");
            ProfitsGross = GetFinancialData(0, "Валовая прибыль");
            ProfitsNet = GetFinancialData(0, "Чистая прибыль");
            Assets = GetFinancialData(1, "Итого активы");
            Obligations = GetFinancialData(1, "Итого обязательства");
            ShareCapitals = GetFinancialData(1, "Итого акционерный капитал");
            CashFlows = GetFinancialData(2, "Чистое изменение денежных средств");
        }

        public List<DateTime> Dates { get; }
        public decimal?[] Revenues { get; }
        public decimal?[] ProfitsGross { get; }
        public decimal?[] ProfitsNet { get; }
        public decimal?[] Assets { get; }
        public decimal?[] Obligations { get; }
        public decimal?[] ShareCapitals { get; }
        public decimal?[] CashFlows { get; }

        private decimal?[] GetFinancialData(int tableIndex, string pattern)
        {
            var error = $"financial data for '{pattern}' is null!";

            var result = new decimal?[] { null, null, null, null };
            var prepareData = page
                .DocumentNode
                .SelectNodes("//table[@class='genTbl openTbl companyFinancialSummaryTbl']/tbody")[tableIndex]
                ?.ChildNodes;

            if (prepareData is null)
                throw new NotSupportedException(error);

            var financialData = prepareData.FirstOrDefault(x => x.Name == "tr" && x.InnerText.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase) > 0);

            if (financialData is null)
                throw new NotSupportedException(error);

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
            var error = "financial page date parsing failed!";

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
    internal class BalancePage
    {
        private readonly HtmlDocument page;
        public BalancePage(HtmlDocument? page)
        {
            this.page = page ?? throw new NullReferenceException("loaded page is null");

            Turnovers = GetBalanceData("Итого оборотные активы");
            LongDebts = GetBalanceData("Общая долгосрочная задолженность по кредитам и займам");
        }

        public decimal?[] Turnovers { get; }
        public decimal?[] LongDebts { get; }

        private decimal?[] GetBalanceData(string pattern)
        {
            var error = $"financial data for '{pattern}' is null!";
            var result = new decimal?[] { null, null, null, null };

            var prepareData = page.DocumentNode
                .SelectNodes("//span[@class]")
                .FirstOrDefault(x => x.InnerText == pattern);

            if (prepareData is null)
                throw new NotSupportedException(error);

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
    internal class DividendPage
    {
        private readonly HtmlDocument page;
        public DividendPage(HtmlDocument? page, IReadOnlyList<DateTime> dates)
        {
            this.page = page ?? throw new NullReferenceException("loaded page is null");

            Dividends = GetDividends(dates);
        }
        public decimal?[] Dividends { get; }

        private decimal?[] GetDividends(IReadOnlyList<DateTime> dates)
        {
            var result = new decimal?[] { null, null, null, null };

            var dividendData = page
                .DocumentNode
                .SelectNodes("//th[@class]")
                .FirstOrDefault(x => x.InnerText == "Экс-дивиденд");

            if (dividendData is null)
                return result;

            var dividendDatesData = dividendData
                .ParentNode
                .ParentNode
                .NextSibling
                .NextSibling
                .ChildNodes
                .Where(x => x.Name == "tr")
                .Select(x => x.ChildNodes.Where(y => y.Name == "td")
                .Select(z => z.InnerText))
                .Select(x => x.FirstOrDefault())
                .ToArray();

            var dividendDates = new List<DateTime>(dividendDatesData.Length);

            foreach (var item in dividendDatesData)
                if (DateTime.TryParse(item, out var date))
                    dividendDates.Add(date);

            if (!dividendDates.Any())
                return result;

            var dividendValueData = dividendData
                .ParentNode?
                .ParentNode?
                .NextSibling?
                .NextSibling?
                .ChildNodes
                .Where(x => x.Name == "tr")
                .Select(x => x.ChildNodes.Where(y => y.Name == "td")
                .Select(z => z.InnerText))
                .Select(x => x.Skip(1)
                .FirstOrDefault())
                .ToArray();

            if (dividendValueData is null) 
                return result;

            for (var i = 0; i < dates.Count; i++)
            {
                var reportYear = dates[i].Year;
                var reportQuarter = CommonHelper.GetQuarter(dates[i].Month);

                for (var j = 0; j < dividendDates.Count; j++)
                {
                    var dividendYear = dividendDates[j].Year;
                    var dividendQuarter = CommonHelper.GetQuarter(dividendDates[j].Month);

                    if (reportYear != dividendYear || reportQuarter != dividendQuarter) 
                        continue;
                    
                    var dividendValue = dividendValueData[j]?.Replace(".", "", StringComparison.CurrentCultureIgnoreCase);

                    if (!decimal.TryParse(dividendValue, out var data)) 
                        continue;
                    
                    if (j > 0)
                    {
                        var previousDividendYear = dividendDates[j - 1].Year;
                        var previousDividendQuarter = CommonHelper.GetQuarter(dividendDates[j - 1].Month);
                        if (dividendYear == previousDividendYear && dividendQuarter == previousDividendQuarter)
                        {
                            var dividendPreviousValue = dividendValueData[j - 1]?.Replace(".", "", StringComparison.CurrentCultureIgnoreCase);
                            if (decimal.TryParse(dividendPreviousValue, out var previousData))
                                data += previousData;
                        }
                    }

                    result[i] = data;
                }
            }

            return result;
        }
    }
}