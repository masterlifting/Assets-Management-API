using CommonServices.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.Services.MapServices;
using IM.Service.Company.Prices.Services.PriceServices.Interfaces;

namespace IM.Service.Company.Prices.Services.PriceServices.Implementations
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
            var priceArray = prices.ToArray();
            var result = new List<Price>(priceArray.Length);

            foreach (var price in priceArray)
            {
                var priceData = await client.GetLastYearPricesAsync(price.TickerName);
                var mappedPrices = priceMapper.MapToPrices(priceData);
                var priceResult = mappedPrices.Where(x => x.Date.Date > price.Date.Date).ToArray();
                if (priceResult.Any())
                    result.AddRange(priceResult);
            }

            return result.ToArray();
        }
        public async Task<Price[]> GetLastPricesToAddAsync(IEnumerable<PriceIdentity> prices) =>
            await GetLastPricesAsync(prices, (newDate, oldDate) => newDate > oldDate);
        public async Task<Price[]> GetLastPricesToUpdateAsync(IEnumerable<PriceIdentity> prices) =>
            await GetLastPricesAsync(prices, (newDate, oldDate) => newDate == oldDate);

        private async Task<Price[]> GetLastPricesAsync(IEnumerable<PriceIdentity> prices, Func<DateTime, DateTime, bool> condition)
        {
            var priceArray = prices is not null ? prices.ToArray() : Array.Empty<PriceIdentity>();

            var result = new List<Price>(priceArray.Length);

            var priceData = await client.GetLastPricesAsync(priceArray.Select(x => x.TickerName));
            var mappedPrices = priceMapper.MapToPrices(priceData);

            foreach (var item in mappedPrices.Join(priceArray, x => x.TickerName, y => y.TickerName, (x, y) => new { Price = x, y.Date.Date }))
            {
                if (condition.Invoke(item.Price.Date.Date, item.Date))
                    result.Add(item.Price);
            }

            return result.ToArray();
        }
    }
}
