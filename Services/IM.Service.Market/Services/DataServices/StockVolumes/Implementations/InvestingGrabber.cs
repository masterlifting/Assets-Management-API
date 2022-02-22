using HtmlAgilityPack;

using IM.Service.Common.Net;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IM.Service.Market.Clients;
using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.DataAccess.Repositories;
using IM.Service.Market.Models.Data;

namespace IM.Service.Market.Services.DataServices.StockVolumes.Implementations;

public class InvestingGrabber : IDataGrabber
{
    private readonly Repository<StockVolume> repository;
    private readonly ILogger<StockVolumeGrabber> logger;
    private readonly InvestingParserHandler handler;

    public InvestingGrabber(Repository<StockVolume> repository, ILogger<StockVolumeGrabber> logger, InvestingClient client)
    {
        this.repository = repository;
        this.logger = logger;
        handler = new(client);
    }

    public async Task GrabCurrentDataAsync(string source, DataConfigModel config) => await GrabHistoryDataAsync(source, config);
    public async Task GrabCurrentDataAsync(string source, IEnumerable<DataConfigModel> configs) => await GrabHistoryDataAsync(source, configs);

    public async Task GrabHistoryDataAsync(string source, DataConfigModel config)
    {
        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException(config.CompanyId);

            var site = await handler.LoadDataAsync(config.SourceValue);
            var result = InvestingParserHandler.Parse(site, config.CompanyId, source);

            await repository.CreateAsync(result, "Investing history stock volumes");
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Place: {place}. Error: {exception}", nameof(InvestingGrabber) + '.' + nameof(GrabCurrentDataAsync), exception.InnerException?.Message ?? exception.Message);
        }
    }
    public async Task GrabHistoryDataAsync(string source, IEnumerable<DataConfigModel> configs)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        foreach (var config in configs)
            if (await timer.WaitForNextTickAsync())
                await GrabHistoryDataAsync(source, config);
    }
}

internal class InvestingParserHandler
{
    private readonly InvestingClient client;
    internal InvestingParserHandler(InvestingClient client) => this.client = client;

    internal async Task<HtmlDocument> LoadDataAsync(string sourceValue) => await client.GetMainPageAsync(sourceValue);

    internal static StockVolume Parse(HtmlDocument site, string companyId, string sourceName)
    {
        var mainPage = new MainPage(site);

        return new StockVolume
        {
            CompanyId = companyId,
            SourceType = sourceName,
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