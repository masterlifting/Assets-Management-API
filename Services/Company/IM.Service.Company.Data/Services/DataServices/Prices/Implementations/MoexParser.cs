using IM.Service.Company.Data.Clients.Price;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.Prices.Interfaces;
using IM.Service.Company.Data.Services.MapServices;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices.Prices.Implementations
{
    public class MoexParser : IPriceParser
    {
        private readonly MoexClient client;
        public MoexParser(MoexClient client) => this.client = client;

        public async Task<Price[]> GetHistoryPricesAsync(string source, DateDataConfigModel config)
        {
            var prices = await client.GetHistoryPricesAsync(config.CompanyId, config.Date);
            return PriceMapper.Map(source, prices);
        }
        public async Task<Price[]> GetHistoryPricesAsync(string source, IEnumerable<DateDataConfigModel> config)
        {
            var dataArray = config.ToArray();
            var result = new List<Price>(dataArray.Length);

            foreach (var item in dataArray)
            {
                var prices = await client.GetHistoryPricesAsync(item.CompanyId, item.Date);
                result.AddRange(PriceMapper.Map(source, prices));
                await Task.Delay(200);
            }

            return result.ToArray();
        }

        public async Task<Price[]> GetLastPricesAsync(string source, DateDataConfigModel config)
        {
            var prices = await client.GetLastPricesAsync();
            return PriceMapper.Map(source, prices, new[] { config.CompanyId });
        }
        public async Task<Price[]> GetLastPricesAsync(string source, IEnumerable<DateDataConfigModel> config)
        {
            var prices = await client.GetLastPricesAsync();
            return PriceMapper.Map(source, prices, config.Select(x => x.CompanyId));
        }
    }
}
