using IM.Service.Shared.RepositoryService;
using IM.Service.Portfolio.Domain.Entities.Catalogs;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class BrokerRepositoryHandler : RepositoryHandler<Broker>
{
    private readonly DatabaseContext context;
    public BrokerRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Broker>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Broker> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Description = New.Description;

        return result.Select(x => x.Old);
    }
    public override IQueryable<Broker> GetExist(IEnumerable<Broker> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Brokers.Where(x => ids.Contains(x.Id));
    }
}