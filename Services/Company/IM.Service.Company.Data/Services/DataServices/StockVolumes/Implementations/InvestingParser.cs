using HtmlAgilityPack;

using IM.Service.Common.Net;
using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.StockVolumes.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices.StockVolumes.Implementations;

public class InvestingParser : IStockVolumeParser
{
    private readonly ILogger<StockVolumeParser> logger;
    private readonly InvestingParserHandler handler;
    public InvestingParser(ILogger<StockVolumeParser> logger, InvestingClient client)
    {
        this.logger = logger;
        handler = new(client);
    }

    public async Task<StockVolume[]> GetStockVolumesAsync(string source, DateDataConfigModel config)
    {
        var result = Array.Empty<StockVolume>();

        try
        {
            if (config.SourceValue is null)
                throw new ArgumentNullException($"Source value for '{config.CompanyId}' is null");

            var site = await handler.LoadDataAsync(config.SourceValue);
            result = InvestingParserHandler.Parse(site, config.CompanyId, source);
        }
        catch (Exception exception)
        {
            logger.LogError(LogEvents.Processing, "Stock volume parser error: {error}", exception.InnerException?.Message ?? exception.Message);
        }

        return result;
    }
    public async Task<StockVolume[]> GetStockVolumesAsync(string source, IEnumerable<DateDataConfigModel> config)
    {
        var _config = config.ToArray();
        var result = new List<StockVolume>(_config.Length * 5);

        foreach (var item in _config)
        {
            result.AddRange(await GetStockVolumesAsync(source, item));
            await Task.Delay(5000);
        }

        return result.ToArray();
    }
}

internal class InvestingParserHandler
{
    private readonly InvestingClient client;
    internal InvestingParserHandler(InvestingClient client) => this.client = client;

    internal async Task<HtmlDocument> LoadDataAsync(string sourceValue) => await client.GetMainPageAsync(sourceValue);

    internal static StockVolume[] Parse(HtmlDocument site, string companyId, string sourceName)
    {
        var result = new StockVolume[4];
        var mainPage = new MainPage(site);

        for (var i = 0; i < result.Length; i++)
            result[i] = new()
            {
                CompanyId = companyId,
                SourceType = sourceName,
                Date = DateTime.UtcNow,
                Value = mainPage.StockVolume
            };

        return result;
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