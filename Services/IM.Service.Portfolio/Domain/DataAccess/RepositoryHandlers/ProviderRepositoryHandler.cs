using IM.Service.Shared.SqlAccess;
using IM.Service.Portfolio.Domain.Entities.Catalogs;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class ProviderRepositoryHandler : RepositoryHandler<Provider>
{
    private readonly DatabaseContext context;
    public ProviderRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Provider>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Provider> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Description = New.Description;

        return result.Select(x => x.Old);
    }
    public override IQueryable<Provider> GetExist(IEnumerable<Provider> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Providers.Where(x => ids.Contains(x.Id));
    }
}