using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.DataAccess.Comparators;
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
    public Task GetCreateHandlerAsync(ref Industry[] entities)
    {
        var exist = GetExist(entities);
        var comparer = new IndustryComparer();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
    
    public Task GetUpdateHandlerAsync(ref Industry entity)
    {
        var ctxEntity = context.Industries.FindAsync(entity.Id).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Industry)} data not found. ");

        ctxEntity.Name = entity.Name;
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

    public async Task<IList<Industry>> GetDeleteHandlerAsync(IReadOnlyCollection<Industry> entities)
    {
        var comparer = new IndustryComparer();
        var result = new List<Industry>();

        foreach (var group in entities.GroupBy(x => x.SectorId))
        {
            var dbEntities = await context.Industries.Where(x => x.SectorId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(Industry entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Industry> entities) => Task.CompletedTask;

    private IQueryable<Industry> GetExist(IEnumerable<Industry> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant())
            .ToArray();

        return context.Industries.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }
}