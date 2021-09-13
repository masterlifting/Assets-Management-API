using CommonServices;

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using IM.Services.Company.Reports.Clients;
using IM.Services.Company.Reports.DataAccess.Entities;
using IM.Services.Company.Reports.Services.ReportServices.Interfaces;

namespace IM.Services.Company.Reports.Services.ReportServices.Implementations
{
    public class InvestingParser : IReportParser
    {
        private readonly InvestingClient client;
        public InvestingParser(InvestingClient client) => this.client = client;

        public async Task<Report[]> GetReportsAsync(Ticker ticker)
        {
            Report[] result = Array.Empty<Report>();

            if (ticker?.SourceValue is null)
            {
                Console.WriteLine($"Отсутствует источник для поиска отчетов по тикеру: '{ticker?.Name}'");
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

        private async Task<HtmlDocument[]> LoadSiteAsync(string sourceValue) => await Task.WhenAll(new Task<HtmlDocument>[]
            {
                client.GetMainPageAsync(sourceValue),
                client.GetFinancialPageAsync(sourceValue),
                client.GetBalancePageAsync(sourceValue),
                client.GetDividendPageAsync(sourceValue)
            });
        private static Report[] GetReports(HtmlDocument[] site, string tickerName)
        {
            var result = new Report[4];
            var culture = CultureInfo.CreateSpecificCulture("ru-RU");

            var mainPage = new MainPage(site[0]);
            var financialPage = new FinancialPage(site[1], culture);
            var balancePage = new BalancePage(site[2]);
            var dividendPage = new DividendPage(site[3], financialPage.Dates.ToArray());

            for (int i = 0; i < 4; i++)
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

    class MainPage
    {
        private readonly HtmlDocument page;
        public MainPage(HtmlDocument page)
        {
            this.page = page;
            StockVolume = GetStockVolume();
        }

        public long StockVolume { get; }

        long GetStockVolume()
        {
            var stockVolumeData = page.DocumentNode.SelectNodes("//dt").FirstOrDefault(x => x.InnerText == "Акции в обращении")?.NextSibling?.InnerText;
            return stockVolumeData is not null && long.TryParse(stockVolumeData.Replace(".", ""), out long result)
                ? result
                : throw new NotSupportedException("stock volume failed parsing");
        }
    }
    class FinancialPage
    {
        private readonly HtmlDocument page;
        public FinancialPage(HtmlDocument page, CultureInfo culture)
        {
            this.page = page;
            Dates = GetDates(culture);
            Revenues = GetFinantialData(0, "Общий доход");
            ProfitsGross = GetFinantialData(0, "Валовая прибыль");
            ProfitsNet = GetFinantialData(0, "Чистая прибыль");
            Assets = GetFinantialData(1, "Итого активы");
            Obligations = GetFinantialData(1, "Итого обязательства");
            ShareCapitals = GetFinantialData(1, "Итого акционерный капитал");
            CashFlows = GetFinantialData(2, "Чистое изменение денежных средств");
        }

        public List<DateTime> Dates { get; }
        public decimal?[] Revenues { get; }
        public decimal?[] ProfitsGross { get; }
        public decimal?[] ProfitsNet { get; }
        public decimal?[] Assets { get; }
        public decimal?[] Obligations { get; }
        public decimal?[] ShareCapitals { get; }
        public decimal?[] CashFlows { get; }

        decimal?[] GetFinantialData(int tableIndex, string pattern)
        {
            var result = new decimal?[] { null, null, null, null };

            var finantialData = page.DocumentNode.SelectNodes("//table[@class='genTbl openTbl companyFinancialSummaryTbl']/tbody")[tableIndex]
            .ChildNodes.Where(x => x.Name == "tr" && x.InnerText.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase) > 0).FirstOrDefault();

            if (finantialData is not null)
            {
                var finantialValueData = finantialData.ChildNodes.Where(x => x.Name == "td").Skip(1).Select(x => x.InnerText).ToArray();
                for (int i = 0; i < finantialValueData.Length; i++)
                    if (decimal.TryParse(finantialValueData[i], out decimal value))
                        result[i] = value;
            }

            return result;
        }
        List<DateTime> GetDates(CultureInfo culture)
        {
            var result = new List<DateTime>(4);

            var dateNode = page.DocumentNode.SelectNodes("//th[@class='arial_11 noBold title right period']").FirstOrDefault();
            if (dateNode is not null)
            {
                var dates = dateNode.ParentNode.InnerText.Split("\n");
                for (int i = 0; i < dates.Length; i++)
                    if (DateTime.TryParse(dates[i], culture, DateTimeStyles.AssumeLocal, out DateTime date))
                        result.Add(date);
            }

            return result.Count < 4 ? throw new NotSupportedException("dates failed parsing") : result;
        }
    }
    class BalancePage
    {
        private readonly HtmlDocument page;
        public BalancePage(HtmlDocument page)
        {
            this.page = page;
            Turnovers = GetBalanceData("Итого оборотные активы");
            LongDebts = GetBalanceData("Общая долгосрочная задолженность по кредитам и займам");
        }

        public decimal?[] Turnovers { get; }
        public decimal?[] LongDebts { get; }

        decimal?[] GetBalanceData(string pattern)
        {
            var result = new decimal?[] { null, null, null, null };

            var balanceData = page.DocumentNode
                .SelectNodes("//span[@class]")
                .Where(x => x.InnerText == pattern)
                .FirstOrDefault()
                .ParentNode?.ParentNode?.ChildNodes
                .Where(x => x.Name == "td")
                .Skip(1)
                .Select(x => x.InnerText)
                .ToArray();

            if (balanceData is not null)
                for (int i = 0; i < balanceData.Length; i++)
                    if (decimal.TryParse(balanceData[i], out decimal value))
                        result[i] = value;

            return result;
        }
    }
    class DividendPage
    {
        private readonly HtmlDocument page;
        public DividendPage(HtmlDocument page, DateTime[] dates)
        {
            this.page = page;
            Dividends = GetDividends(dates);
        }
        public decimal?[] Dividends { get; }
        decimal?[] GetDividends(DateTime[] dates)
        {
            var result = new decimal?[] { null, null, null, null };

            var dividendData = page.DocumentNode.SelectNodes("//th[@class]").Where(x => x.InnerText == "Экс-дивиденд").FirstOrDefault();

            if (dividendData is not null)
            {
                var dividendDatesData = dividendData.ParentNode.ParentNode.NextSibling.NextSibling.ChildNodes.Where(x => x.Name == "tr")
                    .Select(x => x.ChildNodes.Where(y => y.Name == "td")
                    .Select(z => z.InnerText))
                    .Select(x => x.FirstOrDefault());

                if (dividendDatesData is not null)
                {
                    var dividendDates = new List<DateTime>(dividendDatesData.Count());

                    foreach (var item in dividendDatesData)
                        if (DateTime.TryParse(item, out DateTime date))
                            dividendDates.Add(date);

                    if (dividendDates.Any())
                    {
                        var dividendValueData = dividendData.ParentNode?.ParentNode?.NextSibling?.NextSibling?.ChildNodes.Where(x => x.Name == "tr")
                            .Select(x => x.ChildNodes.Where(y => y.Name == "td")
                            .Select(z => z.InnerText))
                            .Select(x => x.Skip(1)
                            .FirstOrDefault());

                        if (dividendValueData is not null)
                        {
                            var dividendValues = dividendValueData.ToArray();

                            for (int i = 0; i < dates.Length; i++)
                            {
                                var reportYear = dates[i].Year;
                                var reportQuarter = CommonHelper.GetQuarter(dates[i].Month);

                                for (int j = 0; j < dividendDates.Count; j++)
                                {
                                    var dividendYear = dividendDates[j].Year;
                                    var dividendQuarter = CommonHelper.GetQuarter(dividendDates[j].Month);

                                    if (reportYear == dividendYear && reportQuarter == dividendQuarter)
                                    {
                                        var dividendValue = dividendValues[j].Replace(".", "", StringComparison.CurrentCultureIgnoreCase);

                                        if (decimal.TryParse(dividendValue, out decimal data))
                                        {
                                            if (j > 0)
                                            {
                                                var previousDividendYear = dividendDates[j - 1].Year;
                                                var previousDividendQuarter = CommonHelper.GetQuarter(dividendDates[j - 1].Month);
                                                if (dividendYear == previousDividendYear && dividendQuarter == previousDividendQuarter)
                                                {
                                                    var dividendPreviousValue = dividendValues[j - 1].Replace(".", "", StringComparison.CurrentCultureIgnoreCase);
                                                    if (decimal.TryParse(dividendPreviousValue, out decimal previousData))
                                                        data += previousData;
                                                }
                                            }

                                            result[i] = data;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}