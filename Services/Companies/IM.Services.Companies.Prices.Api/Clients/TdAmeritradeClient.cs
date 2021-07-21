using IM.Services.Companies.Prices.Api.Models.Client.TdAmeritradeModels;
using IM.Services.Companies.Prices.Api.Settings;
using IM.Services.Companies.Prices.Api.Settings.Client;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api
{
    public partial class TdAmeritradeClient
    {
        private readonly HttpClient httpClient;
        private readonly TdAmeritradeSettings settings;

        public TdAmeritradeClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.TdAmeritradeSettings;
        }

        public async Task<TdAmeritradeLastPriceResultModel> GetLastPricesAsync(IEnumerable<string> tickers)
        {
            if (tickers is null)
                throw new NullReferenceException("tickers is null");

            StringBuilder urlsb = new($"https://{settings.Host}/v1/marketdata/quotes?apikey={settings.ApiKey}&symbol=");

            foreach (var ticker in tickers)
                urlsb.Append($"{ticker}%2C");

            var url = urlsb.ToString();
            url = url.Remove(url.Length - 3, 3);
            var data = await httpClient.GetFromJsonAsync<Dictionary<string, TdAmeritradeLastPriceData>>(url);

            return new(data);
        }
        public async Task<TdAmeritradeHistoryPriceResultModel> GetLastYearPricesAsync(string ticker)
        {
            ticker = ticker.ToUpperInvariant();
            string url = $"https://{settings.Host}/v1/marketdata/{ticker}/pricehistory?apikey={settings.ApiKey}&periodType=year&period=1&frequencyType=daily&frequency=1&needExtendedHoursData=false";
            var data = await httpClient.GetFromJsonAsync<TdAmeritradeHistoryPriceData>(url);
            
            return new(data, ticker);
        }
    }
}