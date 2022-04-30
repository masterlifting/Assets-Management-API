using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class DerivativeRepository : RepositoryHandler<Derivative, DatabaseContext>
{
    private readonly DatabaseContext context;
    public DerivativeRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Derivative>> RunUpdateRangeHandlerAsync(IEnumerable<Derivative> entities)
    {
        entities = entities.ToArray();
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