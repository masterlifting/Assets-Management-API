using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Services.RabbitMq;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Shared.Enums;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class EventRepositoryHandler : RepositoryHandler<Event>
{
    private readonly DatabaseContext context;
    private readonly RabbitAction rabbitAction;
    public EventRepositoryHandler(RabbitAction rabbitAction, DatabaseContext context)
    {
        this.context = context;
        this.rabbitAction = rabbitAction;
    }

    public override async Task<IEnumerable<Event>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Event> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.UpdateTime = DateTime.UtcNow;
            
            Old.TypeId = New.TypeId;
            Old.Date = New.Date;

            Old.Value = New.Value;
            Old.Info = New.Info;
            Old.DerivativeId = New.DerivativeId;
            Old.DerivativeCode = New.DerivativeCode;
            Old.ExchangeId = New.ExchangeId;
            Old.AccountId = New.AccountId;
            Old.UserId = New.UserId;
            Old.ProviderId = New.ProviderId;
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
    public override Task RunPostProcessAsync(RepositoryActions action, Event entity)
    {
        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Portfolio, QueueEntities.Event, RabbitHelper.GetAction(action), entity);
        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Event> entities)
    {
        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Portfolio, QueueEntities.Events, RabbitHelper.GetAction(action), entities);
        return Task.CompletedTask;
    }
}