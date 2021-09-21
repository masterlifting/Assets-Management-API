using CommonServices.RepositoryService;

using IM.Gateway.Companies.DataAccess.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Gateway.Companies.DataAccess.Repository
{
    public class StockSplitRepository : IRepository<StockSplit>
    {
        private readonly DatabaseContext context;
        public StockSplitRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, StockSplit? checkedEntity)> TryCheckEntityAsync(StockSplit entity)
        {
            entity.CompanyTicker = entity.CompanyTicker.ToUpperInvariant().Trim();
            return entity.Divider <= 0 ? (false, entity) : await Task.FromResult((true, entity));
        }
        public async Task<(bool isSuccess, StockSplit[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<StockSplit> entities)
        {
            var arrayEntities = entities.Where(x => x.Divider >= 0).ToArray();

            if (arrayEntities.Any(x => x.Divider <= 0))
                return (false, arrayEntities);

            foreach (var entity in arrayEntities)
                entity.CompanyTicker = entity.CompanyTicker.ToUpperInvariant().Trim();

            return await Task.FromResult((true, arrayEntities));
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
                contextEntity.Date = newEntity.Date;
                contextEntity.Divider = newEntity.Divider;
            }

            return isCompare;
        }
    }
}
