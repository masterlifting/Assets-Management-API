using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IM.Services.Companies.Prices.Api.Models.Client.MoexModels;
using Microsoft.Extensions.Options;

namespace IM.Services.Companies.Prices.Api
{
    public class MoexClient
    {
        private readonly HttpClient httpClient;
        private readonly MoexSettings settings;


        public MoexClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.MoexSettings;
        }

        public async Task<MoexLastPriceResultModel> GetLastPricesAsync()
        {
            var url = $"https://{settings.Host}/iss/engines/stock/markets/shares/boards/TQBR/securities.json";
            var data = await httpClient.GetFromJsonAsync<MoexLastPriceData>(url);
            return new(data);
        }
        public async Task<MoexHistoryPriceResultModel> GetHistoryPricesAsync(string ticker, DateTime date)
        {
            var url = $"https://{settings.Host}/iss/history/engines/stock/markets/shares/securities/{ticker}/candles.json?from={date.Year}-{date.Month}-{date.Day}&interval=24&start=0";
            var data = await httpClient.GetFromJsonAsync<MoexHistoryPriceData>(url);
            return new(data, ticker);
        }
    }
}