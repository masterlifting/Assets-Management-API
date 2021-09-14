using CommonServices.RepositoryService;
using System.Collections.Generic;
using System.Linq;
using IM.Gateway.Recommendations.DataAccess.Entities;

namespace IM.Gateway.Recommendations.DataAccess.Repository
{
    public class TickerRepository : IRepository<Ticker>
    {
        private readonly DatabaseContext context;
        public TickerRepository(DatabaseContext context) => this.context = context;

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
