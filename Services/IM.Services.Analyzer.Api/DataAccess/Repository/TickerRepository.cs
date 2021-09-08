using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess.Entities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class TickerRepository : IRepository<Ticker>
    {
        private readonly AnalyzerContext context;
        public TickerRepository(AnalyzerContext context) => this.context = context;

        public bool TryCheckEntity(Ticker entity, out Ticker? result)
        {
            result = entity;
            return true;
        }
        public bool TryCheckEntities(IEnumerable<Ticker> entities, out Ticker[] result)
        {
            result = entities.ToArray();
            return true;
        }
        public Ticker GetIntersectedContextEntity(Ticker entity) => context.Tickers.Find(entity.Name);
        public IQueryable<Ticker> GetIntersectedContextEntities(IEnumerable<Ticker> entities)
        {
            var names = entities.Select(y => y.Name).ToArray();
            return context.Tickers.Where(x => names.Contains(x.Name));
        }
        public bool UpdateEntity(Ticker oldResult, Ticker newResult) => true;
    }
}
