using IM.Service.Market.Clients;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Clients;
using IM.Service.Shared.Helpers;

using Microsoft.Extensions.Caching.Memory;

using System.Globalization;

using static IM.Service.Market.Enums;
using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Services.Data.Prices.Implementations;

public sealed class MoexGrabber : IDataGrabber<Price>
{
    private const string cacheCurrentKey = "loadedCurrentMoexPrices";
    private const string logPrefix = $"{nameof(MoexGrabber)}.";
   
    private readonly ILogger logger;
    private readonly IMemoryCache cache;
    private readonly MoexClient client;
    
    public MoexGrabber(ILogger logger, IMemoryCache cache, MoexClient client)
    {
        this.logger = logger;
        this.cache = cache;
        this.client = client;
    }

    public async IAsyncEnumerable<Price[]> GetCurrentDataAsync(CompanySource companySource)
    {
        if (companySource.Value is null)
            throw new ArgumentNullException(companySource.Value);

        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        if (cache.TryGetValue(cacheCurrentKey, out Price[] prices))
            yield return prices.Where(x => x.CompanyId.Equals(companySource.CompanyId) && x.Date == date).ToArray();
        else
        {
            var data = await client.GetCurrentPricesAsync();
            var result = Map(logger, data).ToArray();
            cache.Set(cacheCurrentKey, result, TimeSpan.FromSeconds(100));
            yield return result.Where(x => x.CompanyId.Equals(companySource.CompanyId) && x.Date == date).ToArray();
        }
    }
    public async IAsyncEnumerable<Price[]> GetCurrentDataAsync(IEnumerable<CompanySource> companySources)
    {
        var data = await client.GetCurrentPricesAsync();
        var result = Map(logger, data, companySources.Select(x => x.CompanyId).Distinct());
        yield return result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();
    }

    public async IAsyncEnumerable<Price[]> GetHistoryDataAsync(CompanySource companySource)
    {
        if (companySource.Value is null)
            throw new ArgumentNullException(companySource.Value);

        var data = await client.GetHistoryPricesAsync(companySource.CompanyId, DateTime.UtcNow.AddYears(-1));

        yield return Map(logger, data);
    }
    public async IAsyncEnumerable<Price[]> GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));

        var _companySources = companySources.ToArray();
        var currentData = await client.GetCurrentPricesAsync();
        var currentRresult = Map(logger, currentData, _companySources.Select(x => x.CompanyId).Distinct());

        foreach (var companySource in _companySources)
            if (await timer.WaitForNextTickAsync())
                await foreach (var data in GetHistoryDataAsync(companySource))
                    yield return data.Concat(currentRresult).ToArray();
    }

    private static Price[] Map(ILogger logger, MoexCurrentPriceResultModel clientResult, IEnumerable<string>? tickers = null)
    {
        var clientData = clientResult.Data.Marketdata.Data;

        var preparedData = clientData
        .Where(x => x?[0] != null && x?[12] != null && x?[48] != null)
        .Select(x => new
        {
            Ticker = x[0].ToString(),
            Date = x[48].ToString(),
            Price = x[12].ToString()
        })
        .ToArray();

        var moexData = tickers is null
            ? preparedData
            : preparedData.Join(tickers, x => x.Ticker, y => y, (x, _) => x, StringComparer.OrdinalIgnoreCase).ToArray();

        var result = new List<Price>(moexData.Length);

        foreach (var data in moexData)
        {
            var isDateTime = DateTime.TryParse(data.Date, out var dateTime);
            var isPrice = decimal.TryParse(data.Price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price);

            if (isDateTime && isPrice)
                result.Add(new()
                {
                    CompanyId = data.Ticker!,
                    Date = DateOnly.FromDateTime(dateTime),
                    Value = price,
                    ValueTrue = price,
                    CurrencyId = (byte)Currencies.Rub,
                    SourceId = (byte)Sources.Moex,
                    StatusId = (byte)Statuses.New
                });
            else
                logger.LogError(logPrefix + nameof(Map), new InvalidCastException($"Price or Date for '{data.Ticker}' from MOEX Price data not recognized."));
        }

        return result.ToArray();
    }
    private static Price[] Map(ILogger logger, MoexHistoryPriceResultModel clientResult)
    {
        var (moexHistoryPriceData, ticker) = clientResult;

        var clientData = moexHistoryPriceData.History.Data;

        var moexData = clientData
            .Where(x => x?[1] != null && x?[8] != null)
            .Select(x => new
            {
                Date = x[1].ToString(),
                Price = x[8].ToString()
            })
            .ToArray();

        var result = new List<Price>(moexData.Length);

        foreach (var data in moexData)
        {
            var isDateTime = DateTime.TryParse(data.Date, out var dateTime);
            var isPrice = decimal.TryParse(data.Price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price);

            if (isDateTime && isPrice)
                result.Add(new()
                {
                    CompanyId = ticker,
                    Date = DateOnly.FromDateTime(dateTime),
                    Value = price,
                    ValueTrue = price,
                    CurrencyId = (byte)Currencies.Rub,
                    SourceId = (byte)Sources.Moex,
                    StatusId = (byte)Statuses.New
                });
            else
                logger.LogError(logPrefix + nameof(Map), new InvalidCastException($"Price or Date for '{ticker}' from MOEX Price data not recognized."));
        }

        return result.ToArray();
    }
}