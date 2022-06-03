using IM.Service.Shared.RepositoryService;
using IM.Service.Recommendations.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Domain.DataAccess.RepositoryHandlers;

public class CompanyRepositoryHandler : RepositoryHandler<Company>
{
    private readonly DatabaseContext context;
    public CompanyRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Company>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Company> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
            Old.RatingPlace = New.RatingPlace;
            Old.DealValue = New.DealValue;
            Old.DealCost = New.DealCost;
            Old.PriceLast = New.PriceLast;
            Old.PriceAvg = New.PriceAvg;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Company> GetExist(IEnumerable<Company> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Companies.Where(x => ids.Contains(x.Id));
    }
}