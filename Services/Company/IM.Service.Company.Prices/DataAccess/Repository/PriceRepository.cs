using CommonServices.RepositoryService;
using System.Collections.Generic;
using System.Linq;
using IM.Service.Company.Prices.DataAccess.Entities;

namespace IM.Service.Company.Prices.DataAccess.Repository
{
    public class PriceRepository : IRepository<Price>
    {
        private readonly DatabaseContext context;
        public PriceRepository(DatabaseContext context) => this.context = context;

        public bool TryCheckEntity(Price entity, out Price result)
        {
            result = entity;
            return entity is not null && context.Tickers.Any(x => x.Name.Equals(entity.TickerName));
        }
        public bool TryCheckEntities(IEnumerable<Price> entities, out Price[] result)
        {
            result = entities
                .GroupBy(y => (y.TickerName, y.Date))
                .Select(x => x.First())
                .ToArray();

            var tickers = result.Select(y => y.TickerName).Distinct().ToArray();
            var count = context.Tickers.Count(x => tickers.Contains(x.Name));

            return tickers.Length == count;
        }
        public Price GetIntersectedContextEntity(Price entity) => context.Prices.Find(entity.TickerName, entity.Date);
        public IQueryable<Price> GetIntersectedContextEntities(IEnumerable<Price> entities)
        {
            var tickers = entities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            return context.Prices.Where(x => tickers.Contains(x.TickerName));
        }
        public bool UpdateEntity(Price oldResult, Price newResult)
        {
            var isCompare = (oldResult.TickerName, oldResult.Date) == (newResult.TickerName, newResult.Date);

            if (isCompare)
                oldResult.Value = newResult.Value;

            return isCompare;
        }
    }
}
