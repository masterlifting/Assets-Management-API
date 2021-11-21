using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.DataAccess.Entities;

namespace IM.Service.Company.DataAccess.Repository;

public class IndustryRepository : IRepositoryHandler<Industry>
{
    private readonly DatabaseContext context;
    public IndustryRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref Industry entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Industry[] entities, IEqualityComparer<Industry> comparer)
    {
        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
    
    public Task GetUpdateHandlerAsync(ref Industry entity)
    {
        var ctxEntity = context.Industries.FindAsync(entity.Id).GetAwaiter().GetResult();

        ctxEntity!.Name = entity.Name;
        ctxEntity.Description = entity.Description;
        ctxEntity.SectorId = entity.SectorId;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Industry[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => x.Id, y => y.Id,
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
            Old.Description = New.Description;
            Old.SectorId = New.SectorId;
        }

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public Task SetPostProcessAsync(Industry entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(Industry[] entities) => Task.CompletedTask;

    private IQueryable<Industry> GetExist(IEnumerable<Industry> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant())
            .ToArray();

        return context.Industries.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }
}