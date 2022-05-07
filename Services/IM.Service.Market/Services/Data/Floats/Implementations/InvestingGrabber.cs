using HtmlAgilityPack;

using IM.Service.Market.Clients;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;

using System.Globalization;

using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Data.Floats.Implementations;

public sealed class InvestingGrabber : IDataGrabber<Float>
{
    private readonly InvestingParserHandler handler;
    public InvestingGrabber(InvestingClient client) => handler = new(client);

    public async IAsyncEnumerable<Float[]> GetCurrentDataAsync(CompanySource companySource)
    {
        yield break;
    }

    public async IAsyncEnumerable<Float[]> GetCurrentDataAsync(IEnumerable<CompanySource> companySources)
    {
        yield break;
    }

    public async IAsyncEnumerable<Float[]> GetHistoryDataAsync(CompanySource companySource)
    {
        if (companySource.Value is null)
            throw new ArgumentNullException(companySource.CompanyId);

        var site = await handler.LoadDataAsync(companySource.Value);
        var result = InvestingParserHandler.Parse(site, companySource.CompanyId);

        yield return new[] { result };
    }
    public async IAsyncEnumerable<Float[]> GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        foreach (var companySource in companySources)
            if (await timer.WaitForNextTickAsync())
                await foreach (var data in GetHistoryDataAsync(companySource))
                    yield return data;
    }
}
internal class InvestingParserHandler
{
    private readonly InvestingClient client;
    internal InvestingParserHandler(InvestingClient client) => this.client = client;

    internal async Task<HtmlDocument> LoadDataAsync(string sourceValue) => await client.GetMainPageAsync(sourceValue);

    internal static Float Parse(HtmlDocument site, string companyId)
    {
        var mainPage = new MainPage(site);

        return new Float
        {
            CompanyId = companyId,
            SourceId = (byte)Sources.Investing,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Value = mainPage.StockVolume
        };
    }

    private class MainPage
    {
        private readonly HtmlDocument page;
        public MainPage(HtmlDocument? page)
        {
            this.page = page ?? throw new NullReferenceException("Loaded page is null");
            StockVolume = GetStockVolume();
        }

        public long StockVolume { get; }

        private long GetStockVolume()
        {
            var stockVolumeData = page.DocumentNode.SelectNodes("//dt").FirstOrDefault(x => x.InnerText == "Акции в обращении")?.NextSibling?.InnerText;
            return stockVolumeData is not null
                   && long.TryParse(
                       stockVolumeData.Replace(".", "")
                       , NumberStyles.AllowCurrencySymbol, new CultureInfo("Ru-ru"), out var result)
                ? result
                : throw new NotSupportedException("Stock volume parsing is failed");
        }
    }
}