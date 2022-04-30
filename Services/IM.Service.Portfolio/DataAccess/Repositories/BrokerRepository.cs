using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class BrokerRepository : RepositoryHandler<Broker, DatabaseContext>
{
    private readonly DatabaseContext context;
    public BrokerRepository(DatabaseContext context) : base(context) => this.context = context;

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