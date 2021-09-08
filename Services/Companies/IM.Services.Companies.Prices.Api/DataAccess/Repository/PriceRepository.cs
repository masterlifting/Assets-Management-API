using CommonServices.RepositoryService;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Companies.Prices.Api.DataAccess.Repository
{
    public class PriceRepository : IRepository<Price>
    {
        private readonly PricesContext context;
        public PriceRepository(PricesContext context) => this.context = context;

        public bool TryCheckEntity(Price entity, out Price result)
        {
            result = null;
            var isValid = context.Tickers.Where(x => x.Name.Equals(entity.TickerName)).Any();

            if (isValid)
                result = entity;

            return isValid;
        }
        public bool TryCheckEntities(IEnumerable<Price> entities, out Price[] result)
        {
            result = Array.Empty<Price>();

            var tickers = entities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            var count = context.Tickers.Where(x => tickers.Contains(x.Name)).Count();

            var isValid = tickers.Length == count;

            if (isValid)
                result = entities.ToArray();

            return isValid;
        }
        public Price GetIntersectedContextEntity(Price entity) => context.Prices.Find(entity.TickerName, entity.Date);
        public IQueryable<Price> GetIntersectedContextEntities(IEnumerable<Price> entities)
        {
            var tickers = entities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            return context.Prices.Where(x => tickers.Contains(x.TickerName));
        }
        public bool UpdateEntity(Price oldResult, Price newResult)
        {
            var isCompare = (oldResult.TickerName, oldResult.Date) == (oldResult.TickerName, oldResult.Date);

            if (isCompare)
                oldResult.Value = newResult.Value;

            return isCompare;
        }
    }
}
