using IM.Service.Shared.SqlAccess;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class AssetRepositoryHandler : RepositoryHandler<Asset>
{
    private readonly DatabaseContext context;
    public AssetRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Asset>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Asset> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => (x.Id, x.TypeId), y => (y.Id, y.TypeId), (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
            Old.CountryId = New.CountryId;
            Old.Description = New.Description;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Asset> GetExist(IEnumerable<Asset> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        var typeIds = entities
            .GroupBy(x => x.TypeId)
            .Select(x => x.Key);

        return context.Assets.Where(x => ids.Contains(x.Id) && typeIds.Contains(x.TypeId));
    }
}