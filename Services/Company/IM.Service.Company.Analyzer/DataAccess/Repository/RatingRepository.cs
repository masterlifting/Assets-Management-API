using System;
using IM.Service.Common.Net.RepositoryService;

using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.DataAccess.Comparators;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class RatingRepository : IRepositoryHandler<Rating>
{
    private readonly DatabaseContext context;
    public RatingRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref Rating entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Rating[] entities)
    {
        var exist = GetExist(entities);
        
        var comparer = new RatingComparer();
        entities = entities.Distinct(comparer).ToArray();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
    
    public Task GetUpdateHandlerAsync(ref Rating entity)
    {
        var ctxEntity = context.Ratings.FindAsync(entity.Place).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Rating)} data not found. ");

        ctxEntity.Result = entity.Result;
        ctxEntity.UpdateTime = DateTime.UtcNow;

        entity = ctxEntity;

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
            Old.UpdateTime = DateTime.UtcNow;
        }

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public async Task<IList<Rating>> GetDeleteHandlerAsync(IReadOnlyCollection<Rating> entities)
    {
        var comparer = new RatingComparer();
        var result = new List<Rating>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Ratings.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(Rating entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Rating> entities) => Task.CompletedTask;

    private IQueryable<Rating> GetExist(IEnumerable<Rating> entities)
    {
        var existData = entities
            .GroupBy(x => x.Place)
            .Select(x => x.Key)
            .ToArray();

        return context.Ratings.Where(x => existData.Contains(x.Place));
    }
}