using IM.Service.Common.Net.Models.Dto.Http;

using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IM.Service.Company.Data.Models.Client.Price.MoexModels;
using IM.Service.Company.Data.Settings;

namespace IM.Service.Company.Data.Clients.Price;

public class MoexClient
{
    private readonly HttpClient httpClient;
    private readonly HostModel moexSetting;


    public MoexClient(HttpClient httpClient, IOptions<ServiceSettings> options)
    {
        this.httpClient = httpClient;
        moexSetting = options.Value.ClientSettings.Moex;
    }

    public async Task<MoexLastPriceResultModel> GetLastPricesAsync()
    {
        var url = $"https://{moexSetting.Host}/iss/engines/stock/markets/shares/boards/TQBR/securities.json";
        var data = await httpClient.GetFromJsonAsync<MoexLastPriceData>(url);
        return new(data);
    }
    public async Task<MoexHistoryPriceResultModel> GetHistoryPricesAsync(string ticker, DateTime date)
    {
        var url = $"https://{moexSetting.Host}/iss/history/engines/stock/markets/shares/securities/{ticker}/candles.json?from={date.Year}-{date.Month}-{date.Day}&interval=24&start=0";
        var data = await httpClient.GetFromJsonAsync<MoexHistoryPriceData>(url);
        return new(data, ticker);
    }
}