using HtmlAgilityPack;

using IM.Service.Common.Net;
using IM.Service.MarketData.Clients;
using IM.Service.MarketData.Domain.DataAccess;
using IM.Service.MarketData.Domain.Entities;

using System.Globalization;
using IM.Service.MarketData.Domain.Entities.ManyToMany;

namespace IM.Service.MarketData.Services.DataLoaders.Floats.Implementations;

public class InvestingGrabber : IDataGrabber
{
    private readonly Repository<Float> repository;
    private readonly ILogger<DataLoader<Float>> logger;
    private readonly InvestingParserHandler handler;

    public InvestingGrabber(Repository<Float> repository, ILogger<DataLoader<Float>> logger, InvestingClient client)
    {
        this.repository = repository;
        this.logger = logger;
        handler = new(client);
    }

    public async Task GetCurrentDataAsync(CompanySource companySource) => await GetHistoryDataAsync(companySource);
    public async Task GetCurrentDataAsync(IEnumerable<CompanySource> companySources) => await GetHistoryDataAsync(companySources);

    public async Task GetHistoryDataAsync(CompanySource companySource)
    {
        try
        {
            if (companySource.Value is null)
                throw new ArgumentNullException(companySource.CompanyId);

            var site = await handler.LoadDataAsync(companySource.Value);
            var result = InvestingParserHandler.Parse(site, companySource.CompanyId);

            await repository.CreateAsync(result, "Investing history stock volumes");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(InvestingGrabber) + '.' + nameof(GetCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        foreach (var config in companySources)
            if (await timer.WaitForNextTickAsync())
                await GetHistoryDataAsync(config);
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
            SourceId = (byte)Enums.Sources.Investing,
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