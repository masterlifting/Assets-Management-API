using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Market.Models.Client;
using IM.Service.Market.Settings;

using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IM.Service.Market.Clients;

public class TdAmeritradeClient
{
    private readonly HttpClient httpClient;
    private readonly HostModel tdAmeritradeSetting;

    public TdAmeritradeClient(HttpClient httpClient, IOptions<ServiceSettings> options)
    {
        this.httpClient = httpClient;
        tdAmeritradeSetting = options.Value.ClientSettings.TdAmeritrade;
    }

    public async Task<TdAmeritradeLastPriceResultModel> GetCurrentPricesAsync(IEnumerable<string> tickers)
    {
        var tickerArray = tickers.ToArray();

        if (!tickerArray.Any())
            return new(null);

        var url = $"https://{tdAmeritradeSetting.Host}/v1/marketdata/quotes?apikey={tdAmeritradeSetting.ApiKey}&symbol={string.Join("%2C", tickerArray)}";
        var data = await httpClient.GetFromJsonAsync<Dictionary<string, TdAmeritradeLastPriceData>>(url);

        return new(data);
    }
    public async Task<TdAmeritradeHistoryPriceResultModel> GetHistoryPricesAsync(string ticker)
    {
        ticker = ticker.ToUpperInvariant();
        var url = $"https://{tdAmeritradeSetting.Host}/v1/marketdata/{ticker}/pricehistory?apikey={tdAmeritradeSetting.ApiKey}&periodType=year&period=1&frequencyType=daily&frequency=1&needExtendedHoursData=false";
        var data = await httpClient.GetFromJsonAsync<TdAmeritradeHistoryPriceData>(url);
            
        return new(data, ticker);
    }
}