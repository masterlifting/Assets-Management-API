using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class PriceRepository : IRepository<Price>
    {
        private readonly AnalyzerContext context;
        public PriceRepository(AnalyzerContext context) => this.context = context;

        public bool TryCheckEntity(Price entity, out Price? result)
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
            var isCompare = (oldResult.TickerName, oldResult.Date) == (newResult.TickerName, newResult.Date);

            if (isCompare)
            {
                oldResult.Result = newResult.Result;
                oldResult.StatusId = newResult.StatusId;
            }

            return isCompare;
        }
    }
}
