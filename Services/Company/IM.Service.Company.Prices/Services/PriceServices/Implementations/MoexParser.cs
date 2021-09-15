using CommonServices.Models.Entity;

using IM.Service.Company.Prices.Clients;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.Services.MapServices;
using IM.Service.Company.Prices.Services.PriceServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Prices.Services.PriceServices.Implementations
{
    public class MoexParser : IPriceParser
    {
        private readonly MoexClient client;
        public MoexParser(MoexClient client) => this.client = client;

        public async Task<Price[]> GetHistoryPricesAsync(IEnumerable<PriceIdentity> prices)
        {
            var priceArray = prices.ToArray();

            var result = new List<Price>(priceArray.Length);

            foreach (var price in priceArray)
            {
                var priceData = await client.GetHistoryPricesAsync(price.TickerName, price.Date.AddDays(1));
                var mappedPrices = PriceMapper.MapToPrices(priceData);
                
                // ReSharper disable once InvertIf
                if (mappedPrices.Any())
                {
                    var priceResult = mappedPrices.GroupBy(x => x.Date.Date).Select(x => x.First());
                    result.AddRange(priceResult);
                }
            }

            return result.ToArray();
        }
        public async Task<Price[]> GetLastPricesToAddAsync(IEnumerable<PriceIdentity> prices) =>
            await GetLastPricesAsync(prices, (newDate, oldDate) => newDate > oldDate);
        public async Task<Price[]> GetLastPricesToUpdateAsync(IEnumerable<PriceIdentity> prices) =>
            await GetLastPricesAsync(prices, (newDate, oldDate) => newDate == oldDate);

        private async Task<Price[]> GetLastPricesAsync(IEnumerable<PriceIdentity> prices, Func<DateTime, DateTime, bool> condition)
        {
            var priceArray = prices.ToArray();

            var result = new List<Price>(priceArray.Length);

            var priceData = await client.GetLastPricesAsync();
            var mappedPrices = PriceMapper.MapToPrices(priceData, priceArray.Select(x => x.TickerName));

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in mappedPrices.Join(priceArray, x => x.TickerName, y => y.TickerName, (x, y) => new { Price = x, y.Date.Date }))
                if (condition.Invoke(item.Price.Date.Date, item.Date))
                    result.Add(item.Price);

            return result.ToArray();
        }
    }
}
