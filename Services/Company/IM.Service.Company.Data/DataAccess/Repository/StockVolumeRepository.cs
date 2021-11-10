using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Company.Data.DataAccess.Repository
{
    public class StockVolumeRepository : IRepositoryHandler<StockVolume>
    {
        private readonly DatabaseContext context;
        public StockVolumeRepository(DatabaseContext context) => this.context = context;

        public Task GetCreateHandlerAsync(ref StockVolume entity)
        {
            return Task.CompletedTask;
        }
        public Task GetCreateHandlerAsync(ref StockVolume[] entities, IEqualityComparer<StockVolume> comparer)
        {
            var exist = GetExist(entities);

            if (exist.Any())
                entities = entities.Except(exist, comparer).ToArray();

            return Task.CompletedTask;
        }
        public Task GetUpdateHandlerAsync(ref StockVolume entity)
        {
            var dbEntity = context.StockVolumes.FindAsync(entity.CompanyId, entity.Date).GetAwaiter().GetResult();

            dbEntity.Value = entity.Value;

            entity = dbEntity;

            return Task.CompletedTask;
        }
        public Task GetUpdateHandlerAsync(ref StockVolume[] entities)
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

        private IQueryable<StockVolume> GetExist(StockVolume[] entities)
        {
            var dateMin = entities.Min(x => x.Date);
            var dateMax = entities.Max(x => x.Date);

            var existData = entities
                .GroupBy(x => x.CompanyId)
                .Select(x => x.Key)
                .ToArray();

            return context.StockVolumes.Where(x => existData.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
        }
    }
}
