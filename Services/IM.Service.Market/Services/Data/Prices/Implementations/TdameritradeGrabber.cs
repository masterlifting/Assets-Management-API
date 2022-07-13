using IM.Service.Market.Clients;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Clients;

using static IM.Service.Market.Enums;
using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Services.Data.Prices.Implementations;

public sealed class TdameritradeGrabber : IDataGrabber<Price>
{
    private readonly TdAmeritradeClient client;

    public TdameritradeGrabber(TdAmeritradeClient client)
    {
        this.client = client;
    }

    public async IAsyncEnumerable<Price[]> GetCurrentDataAsync(CompanySource companySource)
    {
        if (companySource.Value is null)
            throw new ArgumentNullException(companySource.Value);

        var data = await client.GetCurrentPricesAsync(new[] { companySource.CompanyId });

        var result = Map(data);

        yield return result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();
    }
    public async IAsyncEnumerable<Price[]> GetCurrentDataAsync(IEnumerable<CompanySource> companySources)
    {
        var data = await client.GetCurrentPricesAsync(companySources.Select(x => x.CompanyId).Distinct());

        var result = Map(data);

        yield return result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();
    }

    public async IAsyncEnumerable<Price[]> GetHistoryDataAsync(CompanySource companySource)
    {
        if (companySource.Value is null)
            throw new ArgumentNullException(companySource.Value);

        var data = await client.GetHistoryPricesAsync(companySource.CompanyId);

        yield return Map(data);
    }
    public async IAsyncEnumerable<Price[]> GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));
        var _companySources = companySources.ToArray();
        var currentData = await client.GetCurrentPricesAsync(_companySources.Select(x => x.CompanyId).Distinct());
        var currentResult = Map(currentData);

        foreach (var companySource in _companySources)
            if (await timer.WaitForNextTickAsync())
                await foreach (var data in GetHistoryDataAsync(companySource))
                    yield return data.Concat(currentResult).ToArray();
    }

    private static Price[] Map(TdAmeritradeLastPriceResultModel clientResult) =>
        clientResult.data is null
            ? Array.Empty<Price>()
            : clientResult.data
                .Where(x => x.Value.lastPrice > 0)
                .Select(x => new Price
                {
                    CompanyId = x.Key,
                    Value = x.Value.lastPrice,
                    ValueTrue = x.Value.lastPrice,
                    Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(x.Value.regularMarketTradeTimeInLong).DateTime),
                    CurrencyId = (byte)Currencies.Usd,
                    SourceId = (byte)Sources.Tdameritrade,
                    StatusId = (byte)Statuses.New
                }).ToArray();
    private static Price[] Map(TdAmeritradeHistoryPriceResultModel clientResult) =>
        clientResult.data?.candles is null
            ? Array.Empty<Price>()
            : clientResult.data.candles
                .Where(x => x.high > 0)
                .Select(x => new Price
                {
                    CompanyId = clientResult.ticker,
                    Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(x.datetime).DateTime),
                    Value = x.high,
                    ValueTrue = x.high,
                    CurrencyId = (byte)Currencies.Usd,
                    SourceId = (byte)Sources.Tdameritrade,
                    StatusId = (byte)Statuses.New
                }).ToArray();
}