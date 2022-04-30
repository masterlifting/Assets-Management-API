using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class UnderlyingAssetRepository : RepositoryHandler<UnderlyingAsset, DatabaseContext>
{
    private readonly DatabaseContext context;
    public UnderlyingAssetRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<UnderlyingAsset>> RunUpdateRangeHandlerAsync(IEnumerable<UnderlyingAsset> entities)
    {
        entities = entities.ToArray();
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