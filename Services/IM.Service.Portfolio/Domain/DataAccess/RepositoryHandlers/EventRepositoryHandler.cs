using System;
using IM.Service.Shared.RepositoryService;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class EventRepositoryHandler : RepositoryHandler<Event>
{
    private readonly DatabaseContext context;
    public EventRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Event>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Event> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.UpdateTime = DateTime.UtcNow;
            Old.Date = New.Date;
            Old.Cost = New.CurrencyId;
            Old.Info = New.Info;
            Old.DerivativeId = New.DerivativeId;
            Old.DerivativeCode = New.DerivativeCode;
            Old.ExchangeId = New.ExchangeId;
            Old.AccountId = New.AccountId;
            Old.UserId = New.UserId;
            Old.BrokerId = New.BrokerId;
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