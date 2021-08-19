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

        public async Task<Price[]> GetHistoryPricesAsync(IEnumerable<(string ticker, DateTime priceDate)> data)
        {
            var result = new List<Price>(data.Count());

            foreach (var (ticker, priceDate) in data)
            {
                var priceData = await client.GetLastYearPricesAsync(ticker);
                var prices = priceMapper.MapToPrices(priceData);
                var priceResult = prices.Where(x => x.Date.Date > priceDate.Date);
                if (priceResult.Any())
                    result.AddRange(priceResult);
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

            var priceData = await client.GetLastPricesAsync(data.Select(x => x.ticker));
            var prices = priceMapper.MapToPrices(priceData);

            foreach (var item in prices.Join(data, x => x.TickerName, y => y.ticker, (x, y) => new { Price = x, y.priceDate.Date }))
            {
                if (condition.Invoke(item.Price.Date.Date, item.Date))
                    result.Add(item.Price);
            }

            return result.ToArray();
        }
    }
}
