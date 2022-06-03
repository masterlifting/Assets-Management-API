using IM.Service.Shared.Models.Http;
using IM.Service.Market.Models.Clients;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Market.Clients;

public class TdAmeritradeClient
{
    private readonly HttpClient httpClient;
    private readonly HostModel settings;

    public TdAmeritradeClient(HttpClient httpClient, IOptions<ServiceSettings> options)
    {
        this.httpClient = httpClient;
        settings = options.Value.ClientSettings.TdAmeritrade;
    }

    public async Task<TdAmeritradeLastPriceResultModel> GetCurrentPricesAsync(IEnumerable<string> tickers)
    {
        var tickerArray = tickers.ToArray();

        if (!tickerArray.Any())
            return new(null);

        var url = $"{settings.Schema}://{settings.Host}/v1/marketdata/quotes?apikey={settings.ApiKey}&symbol={string.Join("%2C", tickerArray)}";
        var data = await httpClient.GetFromJsonAsync<Dictionary<string, TdAmeritradeLastPriceData>>(url);

        return new(data);
    }
    public async Task<TdAmeritradeHistoryPriceResultModel> GetHistoryPricesAsync(string ticker)
    {
        ticker = ticker.ToUpperInvariant();
        var url = $"{settings.Schema}://{settings.Host}/v1/marketdata/{ticker}/pricehistory?apikey={settings.ApiKey}&periodType=year&period=1&frequencyType=daily&frequency=1&needExtendedHoursData=false";
        var data = await httpClient.GetFromJsonAsync<TdAmeritradeHistoryPriceData>(url);
            
        return new(data, ticker);
    }
}