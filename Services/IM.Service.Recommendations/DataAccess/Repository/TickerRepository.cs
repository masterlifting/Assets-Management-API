using CommonServices.RepositoryService;
using System.Collections.Generic;
using System.Linq;
using IM.Gateway.Recommendations.DataAccess.Entities;
using System.Threading.Tasks;

namespace IM.Gateway.Recommendations.DataAccess.Repository
{
    public class TickerRepository : IRepositoryHandler<Ticker>
    {
        private readonly DatabaseContext context;
        public TickerRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, Ticker? checkedEntity)> TryCheckEntityAsync(Ticker entity) => await Task.FromResult((true, entity));

        public async Task<(bool isSuccess, Ticker[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<Ticker> entities) => await Task.FromResult((true, entities.ToArray()));
        public async Task<Ticker?> GetAlreadyEntityAsync(Ticker entity) => await context.Tickers.FindAsync(entity.Name);
        public IQueryable<Ticker> GetAlreadyEntitiesQuery(IEnumerable<Ticker> entities)
        {
            var names = entities.Select(y => y.Name).ToArray();
            return context.Tickers.Where(x => names.Contains(x.Name));
        }
        public bool IsUpdate(Ticker contextEntity, Ticker newEntity) => true;
    }
}
