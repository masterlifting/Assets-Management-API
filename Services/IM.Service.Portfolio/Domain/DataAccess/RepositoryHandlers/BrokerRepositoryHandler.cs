using IM.Service.Common.Net.RepositoryService;
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

    public override async Task<IEnumerable<Broker>> RunUpdateRangeHandlerAsync(IEnumerable<Broker> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        return existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, _) => x)
            .ToArray();
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