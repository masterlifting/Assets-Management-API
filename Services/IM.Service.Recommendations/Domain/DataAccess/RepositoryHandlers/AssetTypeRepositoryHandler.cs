using IM.Service.Recommendations.Domain.Entities.Catalogs;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Domain.DataAccess.RepositoryHandlers;

public class AssetTypeRepositoryHandler : RepositoryHandler<AssetType>
{
    private readonly DatabaseContext context;
    public AssetTypeRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<AssetType>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<AssetType> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
            Old.Description = New.Description;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<AssetType> GetExist(IEnumerable<AssetType> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.AssetTypes.Where(x => ids.Contains(x.Id));
    }
}