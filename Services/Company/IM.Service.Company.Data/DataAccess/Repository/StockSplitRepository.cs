using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Company.Data.DataAccess.Repository
{
    public class StockSplitRepository : IRepositoryHandler<StockSplit>
    {
        private readonly DatabaseContext context;
        public StockSplitRepository(DatabaseContext context) => this.context = context;

        public Task GetCreateHandlerAsync(ref StockSplit entity)
        {
            return Task.CompletedTask;
        }
        public Task GetCreateHandlerAsync(ref StockSplit[] entities, IEqualityComparer<StockSplit> comparer)
        {
            var exist = GetExist(entities);

            if (exist.Any())
                entities = entities.Except(exist, comparer).ToArray();

            return Task.CompletedTask;
        }
        public Task GetUpdateHandlerAsync(ref StockSplit entity)
        {
            var dbEntity = context.StockSplits.FindAsync(entity.CompanyId, entity.Date).GetAwaiter().GetResult();

            dbEntity.Value = entity.Value;

            entity = dbEntity;

            return Task.CompletedTask;
        }
        public Task GetUpdateHandlerAsync(ref StockSplit[] entities)
        {
            var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

            var result = exist
                .Join(entities, x => (x.CompanyId, x.Date), y => (y.CompanyId, y.Date),
                    (x, y) => (Old: x, New: y))
                .ToArray();

            foreach (var (Old, New) in result)
                Old.Value = New.Value;

            entities = result.Select(x => x.Old).ToArray();

            return Task.CompletedTask;
        }

        private IQueryable<StockSplit> GetExist(StockSplit[] entities)
        {
            var dateMin = entities.Min(x => x.Date);
            var dateMax = entities.Max(x => x.Date);

            var existData = entities
                .GroupBy(x => x.CompanyId)
                .Select(x => x.Key)
                .ToArray();

            return context.StockSplits.Where(x => existData.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
        }
    }
}
