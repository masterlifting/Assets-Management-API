using CommonServices.RepositoryService;
using System.Collections.Generic;
using System.Linq;
using IM.Service.Company.Prices.DataAccess.Entities;
using static IM.Service.Company.Prices.Enums;

namespace IM.Service.Company.Prices.DataAccess.Repository
{
    public class TickerRepository : IRepository<Ticker>
    {
        private readonly DatabaseContext context;
        public TickerRepository(DatabaseContext context) => this.context = context;

        public bool TryCheckEntity(Ticker entity, out Ticker result)
        {
            result = entity;

            if (context.SourceTypes.Find(entity.SourceTypeId) is null)
                entity.SourceTypeId = (byte)Enums.PriceSourceTypes.Default;

            return true;
        }
        public bool TryCheckEntities(IEnumerable<Ticker> entities, out Ticker[] result)
        {
            result = entities.ToArray();

            var resultNames = result.Select(x => x.Name).ToArray();
            var correctNames = result.Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, _) => x.Name).ToArray();

            if (correctNames.Length == resultNames.Length)
                return true;

            var uncorrectedNames = resultNames.Except(correctNames);
            var uncorrectedEntities = result.Join(uncorrectedNames, x => x.Name, y => y, (x, _) => x).ToArray();

            foreach (var entity in uncorrectedEntities)
                entity.SourceTypeId = (byte)Enums.PriceSourceTypes.Default;

            result = result.Join(correctNames, x => x.Name, y => y, (x, _) => x).Union(uncorrectedEntities).ToArray();

            return true;
        }
        public Ticker GetIntersectedContextEntity(Ticker entity) => context.Tickers.Find(entity.Name);
        public IQueryable<Ticker> GetIntersectedContextEntities(IEnumerable<Ticker> entities)
        {
            var names = entities.Select(y => y.Name).ToArray();
            return context.Tickers.Where(x => names.Contains(x.Name));
        }
        public bool UpdateEntity(Ticker oldResult, Ticker newResult)
        {
            var isCompare = (oldResult.Name == newResult.Name);

            if (isCompare)
                oldResult.SourceTypeId = newResult.SourceTypeId;

            return isCompare;
        }
    }
}
