using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using IM.Services.Company.Prices.Models.Client.TdAmeritradeModels;
using IM.Services.Company.Prices.Settings;
using IM.Services.Company.Prices.Settings.Client;

namespace IM.Services.Company.Prices
{
    public partial class TdAmeritradeClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly HostModel tdAmeritradeSetting;

        public TdAmeritradeClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            tdAmeritradeSetting = options.Value.ClientSettings.TdAmeritrade;
        }

        public async Task<TdAmeritradeLastPriceResultModel> GetLastPricesAsync(IEnumerable<string> tickers)
        {
            var tickerArray = tickers.ToArray();
            
            if (!tickerArray.Any())
                throw new NullReferenceException("tickers not found!");

            StringBuilder urlsb = new($"https://{tdAmeritradeSetting.Host}/v1/marketdata/quotes?apikey={tdAmeritradeSetting.ApiKey}&symbol=");

            foreach (var ticker in tickerArray)
                urlsb.Append($"{ticker}%2C");

            var url = urlsb.ToString();
            url = url.Remove(url.Length - 3, 3);
            var data = await httpClient.GetFromJsonAsync<Dictionary<string, TdAmeritradeLastPriceData>>(url);

            return new(data);
        }
        public async Task<TdAmeritradeHistoryPriceResultModel> GetLastYearPricesAsync(string ticker)
        {
            ticker = ticker.ToUpperInvariant();
            var url = $"https://{tdAmeritradeSetting.Host}/v1/marketdata/{ticker}/pricehistory?apikey={tdAmeritradeSetting.ApiKey}&periodType=year&period=1&frequencyType=daily&frequency=1&needExtendedHoursData=false";
            var data = await httpClient.GetFromJsonAsync<TdAmeritradeHistoryPriceData>(url);
            
            return new(data, ticker);
        }

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}