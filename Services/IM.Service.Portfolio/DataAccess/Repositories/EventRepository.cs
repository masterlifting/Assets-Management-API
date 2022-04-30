using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class EventRepository : RepositoryHandler<Event, DatabaseContext>
{
    private readonly DatabaseContext context;
    public EventRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Event>> RunUpdateRangeHandlerAsync(IEnumerable<Event> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Cost = New.CurrencyId;
            Old.Info = New.Info;
            Old.DerivativeId = New.DerivativeId;
            Old.ExchangeId = New.ExchangeId;
            Old.BrokerId = New.BrokerId;
            Old.UserId = New.UserId;
            Old.AccountName = New.AccountName;
            Old.EventTypeId = New.EventTypeId;
            Old.CurrencyId = New.CurrencyId;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Event> GetExist(IEnumerable<Event> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Events.Where(x => ids.Contains(x.Id));
    }
}