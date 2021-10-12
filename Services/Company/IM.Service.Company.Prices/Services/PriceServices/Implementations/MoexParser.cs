using CommonServices.Models.Entity;

using IM.Service.Company.Prices.Clients;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.Services.MapServices;
using IM.Service.Company.Prices.Services.PriceServices.Interfaces;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Prices.Services.PriceServices.Implementations
{
    public class MoexParser : IPriceParser
    {
        private readonly MoexClient client;
        public MoexParser(MoexClient client) => this.client = client;

        public async Task<Price[]> GetHistoryPricesAsync(string source, PriceIdentity data)
        {
            var prices = await client.GetHistoryPricesAsync(data.TickerName, data.Date);
            return PriceMapper.Map(source, prices);
        }
        public async Task<Price[]> GetHistoryPricesAsync(string source, IEnumerable<PriceIdentity> data)
        {
            var dataArray = data.ToArray();
            var result = new List<Price>(dataArray.Length);

            foreach (var item in dataArray)
            {
                var prices = await client.GetHistoryPricesAsync(item.TickerName, item.Date);
                result.AddRange(PriceMapper.Map(source, prices));
                await Task.Delay(200);
            }

            return result.ToArray();
        }

        public async Task<Price[]> GetLastPricesAsync(string source, PriceIdentity data)
        {
            var prices = await client.GetLastPricesAsync();
            return PriceMapper.Map(source, prices, new[] { data.TickerName });
        }
        public async Task<Price[]> GetLastPricesAsync(string source, IEnumerable<PriceIdentity> data)
        {
            var prices = await client.GetLastPricesAsync();
            return PriceMapper.Map(source, prices, data.Select(x => x.TickerName));
        }
    }
}
