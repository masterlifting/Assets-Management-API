using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IM.Service.Company.Prices.Models.Client.MoexModels;
using IM.Service.Company.Prices.Settings;
using IM.Service.Company.Prices.Settings.Client;

namespace IM.Service.Company.Prices
{
    public class MoexClient : IDisposable
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

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}