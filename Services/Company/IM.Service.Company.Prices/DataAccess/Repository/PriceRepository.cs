using CommonServices.RepositoryService;

using IM.Service.Company.Prices.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Prices.DataAccess.Repository
{
    public class PriceRepository : IRepositoryHandler<Price>
    {
        private readonly DatabaseContext context;
        public PriceRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, Price? checkedEntity)> TryCheckEntityAsync(Price entity)
        {
            var isSuccess = await context.Tickers.AnyAsync(x => x.Name.Equals(entity.TickerName));

            if (!isSuccess)
                return (false, null);

            var sourceType = entity.SourceType.ToLowerInvariant().Trim();
            var isSourceType = await context.SourceTypes.AnyAsync(x => x.Name.Equals(sourceType));

            if (!isSourceType)
                entity.SourceType = nameof(Enums.PriceSourceTypes.Default);

            return (true, entity);
        }

        public async Task<(bool isSuccess, Price[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<Price> entities)
        {
            var result = entities.ToArray();

            result = result
                .GroupBy(y => (y.TickerName, y.Date))
                .Select(x => x.First())
                .ToArray();

            var tickers = result.Select(y => y.TickerName).Distinct().ToArray();
            var count = await context.Tickers.CountAsync(x => tickers.Contains(x.Name));

            if (tickers.Length != count)
                return (false, Array.Empty<Price>());

            var sourceTypes = await context.SourceTypes.Select(x => x.Name).ToArrayAsync();
            foreach (var entity in result)
                if (!sourceTypes.Any(x => x.Equals(entity.SourceType, StringComparison.OrdinalIgnoreCase)))
                    entity.SourceType = nameof(Enums.PriceSourceTypes.Default);

            return (true, result);
        }
        public async Task<Price?> GetAlreadyEntityAsync(Price entity) => await context.Prices.FindAsync(entity.TickerName, entity.Date);
        public IQueryable<Price> GetAlreadyEntitiesQuery(IEnumerable<Price> entities)
        {
            var data = entities.ToArray();

            var dateMin = data.Min(x => x.Date);
            var dateMax = data.Min(x => x.Date);

            var tickers = data
                .GroupBy(x => x.TickerName)
                .Select(x => x.Key)
                .ToArray();

            return context.Prices.Where(x => tickers.Contains(x.TickerName) && x.Date >= dateMin && x.Date <= dateMax);
        }
        public bool IsUpdate(Price contextEntity, Price newEntity)
        {
            var isCompare = (contextEntity.TickerName, contextEntity.Date) == (newEntity.TickerName, newEntity.Date);

            if (isCompare)
            {
                contextEntity.Value = newEntity.Value;
                contextEntity.SourceType = newEntity.SourceType;
            }

            return isCompare;
        }
    }
}
