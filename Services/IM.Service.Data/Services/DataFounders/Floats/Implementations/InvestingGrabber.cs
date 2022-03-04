using HtmlAgilityPack;

using IM.Service.Common.Net;
using IM.Service.Data.Clients;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Models.Services;
using System.Globalization;

namespace IM.Service.Data.Services.DataFounders.Floats.Implementations;

public class InvestingGrabber : IDataGrabber
{
    private readonly Repository<Float> repository;
    private readonly ILogger<StockVolumeGrabber> logger;
    private readonly InvestingParserHandler handler;

    public InvestingGrabber(Repository<Float> repository, ILogger<StockVolumeGrabber> logger, InvestingClient client)
    {
        this.repository = repository;
        this.logger = logger;
        handler = new(client);
    }

    public async Task GrabCurrentDataAsync(DataConfigModel config) => await GrabHistoryDataAsync(config);
    public async Task GrabCurrentDataAsync(IEnumerable<DataConfigModel> configs) => await GrabHistoryDataAsync(configs);

    public async Task GrabHistoryDataAsync(DataConfigModel config)
    {
        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException(config.CompanyId);

            var site = await handler.LoadDataAsync(config.SourceValue);
            var result = InvestingParserHandler.Parse(site, config.CompanyId);

            await repository.CreateAsync(result, "Investing history stock volumes");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(InvestingGrabber) + '.' + nameof(GrabCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GrabHistoryDataAsync(IEnumerable<DataConfigModel> configs)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        foreach (var config in configs)
            if (await timer.WaitForNextTickAsync())
                await GrabHistoryDataAsync(config);
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