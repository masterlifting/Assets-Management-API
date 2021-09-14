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
    public class MoexParser : IPriceParser
    {
        private readonly MoexClient client;
        private readonly PriceMapper priceMapper;

        public MoexParser(MoexClient client, PriceMapper priceMapper)
        {
            this.client = client;
            this.priceMapper = priceMapper;
        }

        public async Task<Price[]> GetHistoryPricesAsync(IEnumerable<PriceIdentity> prices)
        {
            var result = new List<Price>(prices.Count());

            foreach (var price in prices)
            {
                var priceData = await client.GetHistoryPricesAsync(price.TickerName, price.Date.AddDays(1));
                var _prices = priceMapper.MapToPrices(priceData);
                if (_prices.Any())
                {
                    var priceResult = _prices.GroupBy(x => x.Date.Date).Select(x => x.First());
                    result.AddRange(priceResult);
                }
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

            var priceData = await client.GetLastPricesAsync();
            var _prices = priceMapper.MapToPrices(priceData, prices.Select(x => x.TickerName));

            foreach (var item in _prices.Join(prices, x => x.TickerName, y => y.TickerName, (x, y) => new { Price = x, y.Date.Date }))
            {
                if (condition.Invoke(item.Price.Date.Date, item.Date))
                    result.Add(item.Price);
            }
            return result.ToArray();
        }
    }
}
