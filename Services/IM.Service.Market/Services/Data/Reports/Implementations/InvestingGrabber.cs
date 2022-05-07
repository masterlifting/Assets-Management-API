using HtmlAgilityPack;

using IM.Service.Market.Clients;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;

using System.Globalization;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helpers.LogicHelper;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Data.Reports.Implementations;

public sealed class InvestingGrabber : IDataGrabber<Report>
{
    private readonly InvestingParserHandler handler;
    public InvestingGrabber(InvestingClient client) => handler = new(client);

    public async IAsyncEnumerable<Report[]> GetCurrentDataAsync(CompanySource companySource)
    {
        yield break;
    }

    public async IAsyncEnumerable<Report[]> GetCurrentDataAsync(IEnumerable<CompanySource> companySources)
    {
        yield break;
    }

    public async IAsyncEnumerable<Report[]> GetHistoryDataAsync(CompanySource companySource)
    {
        if (companySource.Value is null)
            throw new ArgumentNullException(companySource.CompanyId);

        var data = await handler.LoadSiteAsync(companySource.Value);
        var result = InvestingParserHandler.Parse(data, companySource.CompanyId);

        yield return result.ToArray();
    }
    public async IAsyncEnumerable<Report[]> GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

        foreach (var companySource in companySources)
            if (await timer.WaitForNextTickAsync())
                await foreach (var data in GetHistoryDataAsync(companySource))
                    yield return data;
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

    internal static IEnumerable<Report> Parse(IReadOnlyList<HtmlDocument> site, string companyId)
    {
        var culture = new CultureInfo("ru-RU");

        var financialPage = new FinancialPage(site[0], culture);
        var balancePage = new BalancePage(site[1], culture, financialPage.ReportsCount);

        var result = new List<Report>(financialPage.ReportsCount);

        for (var i = 0; i < financialPage.ReportsCount; i++)
            result.Add(new()
            {
                CompanyId = companyId,
                SourceId = (byte)Sources.Investing,
                Year = financialPage.Dates[i].Year,
                Quarter = QuarterHelper.GetQuarter(financialPage.Dates[i].Month),
                CurrencyId = (byte)Currencies.Usd,
                StatusId = (byte)Statuses.New,

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