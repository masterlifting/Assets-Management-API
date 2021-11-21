using System;
using IM.Service.Common.Net.RepositoryService;

using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.DataAccess.Repository
{
    public class RatingRepository : IRepositoryHandler<Rating>
    {
        private readonly DatabaseContext context;
        public RatingRepository(DatabaseContext context) => this.context = context;

        public Task GetCreateHandlerAsync(ref Rating entity)
        {
            return Task.CompletedTask;
        }
        public Task GetCreateHandlerAsync(ref Rating[] entities, IEqualityComparer<Rating> comparer)
        {
            var exist = GetExist(entities);

            if (exist.Any())
                entities = entities.Except(exist, comparer).ToArray();

            return Task.CompletedTask;
        }
        public Task GetUpdateHandlerAsync(ref Rating entity)
        {
            var dbEntity = context.Ratings.FindAsync(entity.Place).GetAwaiter().GetResult();

            dbEntity.Result = entity.Result;
            dbEntity.ResultPrice = entity.ResultPrice;
            dbEntity.ResultReport = entity.ResultReport;
            dbEntity.UpdateTime = DateTime.UtcNow;

            entity = dbEntity;

            return Task.CompletedTask;
        }
        public Task GetUpdateHandlerAsync(ref Rating[] entities)
        {
            var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

            var result = exist
                .Join(entities, x => x.Place, y => y.Place,
                    (x, y) => (Old: x, New: y))
                .ToArray();

            foreach (var (Old, New) in result)
            {
                Old.Result = New.Result;
                Old.ResultPrice = New.ResultPrice;
                Old.ResultReport = New.ResultReport;
                Old.UpdateTime = DateTime.UtcNow;
            }

            entities = result.Select(x => x.Old).ToArray();

            return Task.CompletedTask;
        }

        public Task SetPostProcessAsync(Rating entity) => Task.CompletedTask;
        public Task SetPostProcessAsync(Rating[] entities) => Task.CompletedTask;

        private IQueryable<Rating> GetExist(IEnumerable<Rating> entities)
        {
            var existData = entities
                .GroupBy(x => x.Place)
                .Select(x => x.Key)
                .ToArray();

            return context.Ratings.Where(x => existData.Contains(x.Place));
        }
    }
}
