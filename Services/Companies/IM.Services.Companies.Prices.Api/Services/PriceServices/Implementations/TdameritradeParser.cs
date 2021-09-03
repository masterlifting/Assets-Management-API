using CommonServices.Models.Entity;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.MapServices;
using IM.Services.Companies.Prices.Api.Services.PriceServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.PriceServices.Implementations
{
    public class TdameritradeParser : IPriceParser
    {
        private readonly TdAmeritradeClient client;
        private readonly PriceMapper priceMapper;

        public TdameritradeParser(TdAmeritradeClient client, PriceMapper priceMapper)
        {
            this.client = client;
            this.priceMapper = priceMapper;
        }

        public async Task<Price[]> GetHistoryPricesAsync(IEnumerable<PriceIdentity> prices)
        {
            var result = new List<Price>(prices.Count());

            foreach (var price in prices)
            {
                var priceData = await client.GetLastYearPricesAsync(price.TickerName);
                var _prices = priceMapper.MapToPrices(priceData);
                var priceResult = _prices.Where(x => x.Date.Date > price.Date.Date);
                if (priceResult.Any())
                    result.AddRange(priceResult);
            }

            return result.ToArray();
        }
        public async Task<Price[]> GetLastPricesToAddAsync(IEnumerable<PriceIdentity> prices) =>
            await GetLastPricesAsync(prices, (DateTime newDate, DateTime oldDate) => newDate > oldDate);
        public async Task<Price[]> GetLastPricesToUpdateAsync(IEnumerable<PriceIdentity> prices) =>
            await GetLastPricesAsync(prices, (DateTime newDate, DateTime oldDate) => newDate == oldDate);

        private async Task<Price[]> GetLastPricesAsync(IEnumerable<PriceIdentity> prices, Func<DateTime, DateTime, bool> condition)
        {
            var result = new List<Price>(prices.Count());

            var priceData = await client.GetLastPricesAsync(prices.Select(x => x.TickerName));
            var _prices = priceMapper.MapToPrices(priceData);

            foreach (var item in _prices.Join(prices, x => x.TickerName, y => y.TickerName, (x, y) => new { Price = x, y.Date.Date }))
            {
                if (condition.Invoke(item.Price.Date.Date, item.Date))
                    result.Add(item.Price);
            }

            return result.ToArray();
        }
    }
}
