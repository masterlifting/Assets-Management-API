using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class UnderlyingAssetRepositoryHandler : RepositoryHandler<UnderlyingAsset>
{
    private readonly DatabaseContext context;
    public UnderlyingAssetRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<UnderlyingAsset>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<UnderlyingAsset> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.UnderlyingAssetTypeId = New.UnderlyingAssetTypeId;
            Old.CountryId = New.CountryId;
            Old.Name = New.Name;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<UnderlyingAsset> GetExist(IEnumerable<UnderlyingAsset> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.UnderlyingAssets.Where(x => ids.Contains(x.Id));
    }
}