using IM.Service.Common.Net.RepositoryService;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Entities;

namespace IM.Service.Company.Data.DataAccess.Repository
{
    public class PriceRepository : IRepositoryHandler<Price>
    {
        private readonly DatabaseContext context;
        public PriceRepository(DatabaseContext context) => this.context = context;

        public Task GetCreateHandlerAsync(ref Price entity)
        {
            return Task.CompletedTask;
        }
        public Task GetCreateHandlerAsync(ref Price[] entities, IEqualityComparer<Price> comparer)
        {
            var exist = GetExist(entities);

            if (exist.Any())
                entities = entities.Except(exist, comparer).ToArray();

            return Task.CompletedTask;
        }
        public Task GetUpdateHandlerAsync(ref Price entity)
        {
            var dbEntity = context.Prices.FindAsync(entity.CompanyId, entity.Date).GetAwaiter().GetResult();

            dbEntity.Value = entity.Value;

            entity = dbEntity;

            return Task.CompletedTask;
        }
        public Task GetUpdateHandlerAsync(ref Price[] entities)
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

        private IQueryable<Price> GetExist(Price[] entities)
        {
            var dateMin = entities.Min(x => x.Date);
            var dateMax = entities.Max(x => x.Date);

            var existData = entities
                .GroupBy(x => x.CompanyId)
                .Select(x => x.Key)
                .ToArray();

            return context.Prices.Where(x => existData.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
        }
    }
}
