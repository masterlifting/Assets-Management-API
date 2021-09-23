using CommonServices.RepositoryService;

using IM.Gateway.Companies.DataAccess.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IM.Gateway.Companies.DataAccess.Repository
{
    public class StockSplitRepository : IRepository<StockSplit>
    {
        private readonly DatabaseContext context;
        public StockSplitRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, StockSplit? checkedEntity)> TryCheckEntityAsync(StockSplit entity)
        {
            entity.CompanyTicker = entity.CompanyTicker.ToUpperInvariant().Trim();
            var isCompanyContains = await context.Companies.AnyAsync(x => x.Ticker.Equals(entity.CompanyTicker));

            return (isCompanyContains && entity.Value > 0, entity);
        }
        public async Task<(bool isSuccess, StockSplit[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<StockSplit> entities)
        {
            var arrayEntities = entities.ToArray();

            var result = arrayEntities.Where(x => x.Divider >= 0).ToArray();

            if (result.Any(x => x.Value <= 0))
                return (false, arrayEntities);

            foreach (var entity in result)
                entity.CompanyTicker = entity.CompanyTicker.ToUpperInvariant().Trim();

            var tickers = result.GroupBy(y => y.CompanyTicker).Select(y => y.Key).ToArray();
            var count = await context.Companies.CountAsync(x => tickers.Contains(x.Ticker));

            return (tickers.Length == count, result);
        }
        public async Task<StockSplit?> GetAlreadyEntityAsync(StockSplit entity) => await context.StockSplits.FindAsync(entity.CompanyTicker, entity.Date);
        public IQueryable<StockSplit> GetAlreadyEntitiesQuery(IEnumerable<StockSplit> entities)
        {
            var names = entities.Select(y => y.CompanyTicker).ToArray();
            return context.StockSplits.Where(x => names.Contains(x.CompanyTicker));
        }
        public bool IsUpdate(StockSplit contextEntity, StockSplit newEntity)
        {
            var isCompare = (contextEntity.CompanyTicker, contextEntity.Date) == (newEntity.CompanyTicker, newEntity.Date);

            if (isCompare)
            {
                contextEntity.Divider = newEntity.Divider;
                contextEntity.Value = newEntity.Value;
            }

            return isCompare;
        }
    }
}
