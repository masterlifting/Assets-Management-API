using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.MapServices;
using IM.Services.Companies.Prices.Api.Services.PriceServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.PriceServices.Implementations
{
    public class MoexParser : IPriceParser
    {
        private readonly MoexClient client;
        private readonly PriceMapper priceMapper;

        public MoexParser(MoexClient client, PriceMapper priceMapper)
        {
            this.client = client;
            this.priceMapper = priceMapper;
        }

        public async Task<Price[]> GetHistoryPricesAsync(IEnumerable<(string ticker, DateTime priceDate)> data)
        {
            var result = new List<Price>(data.Count());

            foreach (var (ticker, priceDate) in data)
            {
                var priceData = await client.GetHistoryPricesAsync(ticker, priceDate.AddDays(1));
                var prices = priceMapper.MapToPrices(priceData);
                if (prices.Any())
                {
                    var priceResult = prices.GroupBy(x => x.Date.Date).Select(x => x.First());
                    result.AddRange(priceResult);
                }
            }

            return result.ToArray();
        }
        public async Task<Price[]> GetLastPricesToAddAsync(IEnumerable<(string ticker, DateTime priceDate)> data) =>
            await GetLastPricesAsync(data, (DateTime newDate, DateTime oldDate) => newDate > oldDate);
        public async Task<Price[]> GetLastPricesToUpdateAsync(IEnumerable<(string ticker, DateTime priceDate)> data) =>
            await GetLastPricesAsync(data, (DateTime newDate, DateTime oldDate) => newDate == oldDate);

        private async Task<Price[]> GetLastPricesAsync(IEnumerable<(string ticker, DateTime priceDate)> data, Func<DateTime, DateTime, bool> condition)
        {
            var result = new List<Price>(data.Count());

            var priceData = await client.GetLastPricesAsync();
            var prices = priceMapper.MapToPrices(priceData, data.Select(x => x.ticker));

            foreach (var item in prices.Join(data, x => x.TickerName, y => y.ticker, (x, y) => new { Price = x, y.priceDate.Date }))
            {
                if (condition.Invoke(item.Price.Date.Date, item.Date))
                    result.Add(item.Price);
            }
            return result.ToArray();
        }
    }
}
