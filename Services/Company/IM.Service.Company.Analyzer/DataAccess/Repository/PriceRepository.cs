using CommonServices.RepositoryService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Company.Analyzer.DataAccess.Repository
{
    public class PriceRepository : IRepository<Price>
    {
        private readonly DatabaseContext context;
        public PriceRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, Price? checkedEntity)>TryCheckEntityAsync(Price entity) =>
            (await context.Tickers.AnyAsync(x => x.Name.Equals(entity.TickerName)), entity);
        public async Task<(bool isSuccess, Price[] checkedEntities)>TryCheckEntitiesAsync(IEnumerable<Price> entities)
        {
            var result = entities.ToArray();

            result = result
                .GroupBy(y => (y.TickerName, y.Date))
                .Select(x => x.First())
                .ToArray();

            var tickers = result.Select(y => y.TickerName).Distinct().ToArray();
            var count = await context.Tickers.CountAsync(x => tickers.Contains(x.Name));

            return (tickers.Length == count, result);
        }
        public async Task<Price?>GetAlreadyEntityAsync(Price entity) => await context.Prices.FindAsync(entity.TickerName, entity.Date);
        public IQueryable<Price> GetAlreadyEntitiesQuery(IEnumerable<Price> entities)
        {
            var tickers = entities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            return context.Prices.Where(x => tickers.Contains(x.TickerName));
        }
        public bool IsUpdate(Price contextEntity, Price newEntity)
        {
            var isCompare = (contextEntity.TickerName, contextEntity.Date) == (newEntity.TickerName, newEntity.Date);

            if (isCompare)
            {
                contextEntity.Result = newEntity.Result;
                contextEntity.StatusId = newEntity.StatusId;
            }

            return isCompare;
        }
    }
}
