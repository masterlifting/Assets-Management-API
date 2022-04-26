using IM.Service.Market.Clients;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Clients;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.DataLoaders.Prices.Implementations;

public class TdameritradeGrabber : IDataGrabber<Price>
{
    private readonly TdAmeritradeClient client;
    public TdameritradeGrabber(TdAmeritradeClient client) => this.client = client;

    public async IAsyncEnumerable<Price[]> GetCurrentDataAsync(CompanySource companySource)
    {
        if (companySource.Value is null)
            throw new ArgumentNullException(companySource.CompanyId);

        var data = await client.GetCurrentPricesAsync(new[] { companySource.CompanyId });

        var result = Map(data);

        yield return result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();
    }
    public async IAsyncEnumerable<Price[]> GetCurrentDataAsync(IEnumerable<CompanySource> companySources)
    {
        var data = await client.GetCurrentPricesAsync(companySources.Select(x => x.CompanyId));

        var result = Map(data);

        yield return result.Where(x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow)).ToArray();
    }

    public async IAsyncEnumerable<Price[]> GetHistoryDataAsync(CompanySource companySource)
    {
        if (companySource.Value is null)
            throw new ArgumentNullException(companySource.CompanyId);

        var data = await client.GetHistoryPricesAsync(companySource.CompanyId);

        yield return Map(data);
    }
    public async IAsyncEnumerable<Price[]> GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));

        foreach (var companySource in companySources)
            if (await timer.WaitForNextTickAsync())
                await foreach (var data in GetHistoryDataAsync(companySource))
                    yield return data;
    }

    private static Price[] Map(TdAmeritradeLastPriceResultModel clientResult) =>
        clientResult.data is null
            ? Array.Empty<Price>()
            : clientResult.data.Select(x => new Price
            {
                CompanyId = x.Key,
                Value = x.Value.lastPrice,
                Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(x.Value.regularMarketTradeTimeInLong).DateTime),
                CurrencyId = (byte)Currencies.Usd,
                SourceId = (byte)Sources.Tdameritrade,
                StatusId = (byte)Statuses.New
            }).ToArray();
    private static Price[] Map(TdAmeritradeHistoryPriceResultModel clientResult) =>
        clientResult.data?.candles is null
            ? Array.Empty<Price>()
            : clientResult.data.candles.Select(x => new Price
            {
                CompanyId = clientResult.ticker,
                Date = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(x.datetime).DateTime),
                Value = x.high,
                CurrencyId = (byte)Currencies.Usd,
                SourceId = (byte)Sources.Tdameritrade,
                StatusId = (byte)Statuses.New
            }).ToArray();
}