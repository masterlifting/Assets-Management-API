using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.DataAccess.Entities;

namespace IM.Service.Company.DataAccess.Repository;

public class SectorRepository : IRepositoryHandler<Sector>
{
    private readonly DatabaseContext context;
    public SectorRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref Sector entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Sector[] entities, IEqualityComparer<Sector> comparer)
    {
        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
        
    public Task GetUpdateHandlerAsync(ref Sector entity)
    {
        var ctxEntity = context.Sectors.FindAsync(entity.Id).GetAwaiter().GetResult();

        ctxEntity!.Name = entity.Name;
        ctxEntity.Description = entity.Description;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Sector[] entities)
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
        }

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public Task SetPostProcessAsync(Sector entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(Sector[] entities) => Task.CompletedTask;

    private IQueryable<Sector> GetExist(IEnumerable<Sector> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant())
            .ToArray();

        return context.Sectors.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }
}