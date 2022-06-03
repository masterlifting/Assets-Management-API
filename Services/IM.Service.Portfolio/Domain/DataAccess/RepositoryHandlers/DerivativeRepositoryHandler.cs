using IM.Service.Shared.RepositoryService;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class DerivativeRepositoryHandler : RepositoryHandler<Derivative>
{
    private readonly DatabaseContext context;
    public DerivativeRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Derivative>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Derivative> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => (x.Id, x.Code), y => (y.Id, y.Code), (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.UnderlyingAssetId = New.UnderlyingAssetId;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Derivative> GetExist(IEnumerable<Derivative> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);
        var codes = entities
            .GroupBy(x => x.Code)
            .Select(x => x.Key);

        return context.Derivatives.Where(x => ids.Contains(x.Id) && codes.Contains(x.Code));
    }
}