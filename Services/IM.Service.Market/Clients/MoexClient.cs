using IM.Service.Market.Models.Clients;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;
using IM.Service.Shared.Models.Configuration;

namespace IM.Service.Market.Clients;

public class MoexClient
{
    private readonly HttpClient httpClient;
    private readonly HostModel settings;


    public MoexClient(HttpClient httpClient, IOptions<ServiceSettings> options)
    {
        this.httpClient = httpClient;
        settings = options.Value.ClientSettings.Moex;
    }

    public async Task<MoexCurrentPriceResultModel> GetCurrentPricesAsync()
    {
        var url = $"{settings.Schema}://{settings.Host}/iss/engines/stock/markets/shares/boards/TQBR/securities.json";
        var data = await httpClient.GetFromJsonAsync<MoexCurrentPriceData>(url);

        return data?.Marketdata is null 
            ? new(new(new(Array.Empty<object[]>()))) 
            : new(data);
    }
    public async Task<MoexHistoryPriceResultModel> GetHistoryPricesAsync(string ticker, DateTime date)
    {
        var urlFunc = (int start) => $"{settings.Schema}://{settings.Host}/iss/history/engines/stock/markets/shares/boards/tqbr/securities/{ticker}/candles.json?from={date.Year}-{date.Month}-{date.Day}&interval=24&start={start}";
        var firstData = await httpClient.GetFromJsonAsync<MoexHistoryPriceData>(urlFunc(0));

        if (firstData?.History is null)
            return new(new(new(Array.Empty<object[]>())), ticker);

        var data = firstData.History.Data;

        if (data.Length < 100)
            return new(new(new(data)), ticker);

        while (true)
        {
            var partialData = await httpClient.GetFromJsonAsync<MoexHistoryPriceData>(urlFunc(data.Length));

            if (partialData?.History is null)
                break;

            data = data.Concat(partialData.History.Data).ToArray();
            
            if (partialData.History.Data.Length < 100)
                break;
        }

        return new(new(new(data)), ticker);
    }
}