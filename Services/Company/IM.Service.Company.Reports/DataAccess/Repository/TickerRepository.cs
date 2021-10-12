using CommonServices.RepositoryService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Reports.DataAccess.Entities;
using static IM.Service.Company.Reports.Enums;

namespace IM.Service.Company.Reports.DataAccess.Repository
{
    public class TickerRepository : IRepositoryHandler<Ticker>
    {
        private readonly DatabaseContext context;
        public TickerRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, Ticker? checkedEntity)> TryCheckEntityAsync(Ticker entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.SourceValue))
                entity.SourceValue = entity.SourceValue.ToLowerInvariant();

            if (await context.SourceTypes.FindAsync(entity.SourceTypeId) is null)
                entity.SourceTypeId = (byte)ReportSourceTypes.Default;

            return (true, entity);
        }
        public async Task<(bool isSuccess, Ticker[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<Ticker> entities)
        {
            var result = entities.ToArray();

            var correctNames = result.Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, _) => x.Name).ToArray();
            var resultNames = result.Select(x => x.Name).ToArray();

            if (correctNames.Length == resultNames.Length)
                return (true, result);

            var uncorrectedNames = resultNames.Except(correctNames);
            var uncorrectedEntities = result.Join(uncorrectedNames, x => x.Name, y => y, (x, _) => x).ToArray();

            foreach (var entity in uncorrectedEntities)
                entity.SourceTypeId = (byte)ReportSourceTypes.Default;

            result = result.Join(correctNames, x => x.Name, y => y, (x, _) => x).Union(uncorrectedEntities).ToArray();

            foreach (var item in result)
                if (!string.IsNullOrWhiteSpace(item.SourceValue))
                    item.SourceValue = item.SourceValue.ToLowerInvariant();

            return await Task.FromResult((true, result));
        }
        public async Task<Ticker?> GetAlreadyEntityAsync(Ticker entity) => await context.Tickers.FindAsync(entity.Name);
        public IQueryable<Ticker> GetAlreadyEntitiesQuery(IEnumerable<Ticker> entities)
        {
            var names = entities.Select(y => y.Name).ToArray();
            return context.Tickers.Where(x => names.Contains(x.Name));
        }
        public bool IsUpdate(Ticker contextEntity, Ticker newEntity)
        {
            var isCompare = (contextEntity.Name == newEntity.Name);

            // ReSharper disable once InvertIf
            if (isCompare)
            {
                contextEntity.SourceTypeId = newEntity.SourceTypeId;
                contextEntity.SourceValue = newEntity.SourceValue;
            }

            return isCompare;
        }
    }
}
