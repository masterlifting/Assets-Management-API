using CommonServices.Models.Http;

using IM.Service.Company.Prices.Models.Client.TdAmeritradeModels;
using IM.Service.Company.Prices.Settings;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace IM.Service.Company.Prices.Clients
{
    public class TdAmeritradeClient : IDisposable
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
                return new(null);

            StringBuilder urlBuilder = new($"https://{tdAmeritradeSetting.Host}/v1/marketdata/quotes?apikey={tdAmeritradeSetting.ApiKey}&symbol=");

            foreach (var ticker in tickerArray)
                urlBuilder.Append($"{ticker}%2C");

            var url = urlBuilder.ToString();
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