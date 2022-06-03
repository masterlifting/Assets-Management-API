using IM.Service.Shared.RepositoryService;
using IM.Service.Recommendations.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Domain.DataAccess.RepositoryHandlers;

public class SaleRepositoryHandler : RepositoryHandler<Sale>
{
    private readonly DatabaseContext context;
    public SaleRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Sale>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Sale> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Price = New.Price;
            Old.Value = New.Value;
            Old.Plan = New.Plan;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Sale> GetExist(IEnumerable<Sale> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Sales.Where(x => ids.Contains(x.Id));
    }
}