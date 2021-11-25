using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.DataAccess.Comparators;
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
    public Task GetCreateHandlerAsync(ref Sector[] entities)
    {
        var exist = GetExist(entities);
        var comparer = new SectorComparer();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
        
    public Task GetUpdateHandlerAsync(ref Sector entity)
    {
        var ctxEntity = context.Sectors.FindAsync(entity.Id).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Sector)} data not found. ");

        ctxEntity.Name = entity.Name;
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

    public async Task<IList<Sector>> GetDeleteHandlerAsync(IReadOnlyCollection<Sector> entities)
    {
        var comparer = new SectorComparer();
        var result = new List<Sector>();

        foreach (var group in entities.GroupBy(x => x.Name))
        {
            var dbEntities = await context.Sectors.Where(x => x.Name.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(Sector entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Sector> entities) => Task.CompletedTask;

    private IQueryable<Sector> GetExist(IEnumerable<Sector> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant())
            .ToArray();

        return context.Sectors.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }
}