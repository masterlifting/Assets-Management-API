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

public class DealRepositoryHandler : RepositoryHandler<Deal>
{
    private readonly RabbitAction rabbitAction;
    private readonly DatabaseContext context;

    public DealRepositoryHandler(RabbitAction rabbitAction, DatabaseContext context)
    {
        this.rabbitAction = rabbitAction;
        this.context = context;
    }

    public override async Task<IEnumerable<Deal>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Deal> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.UpdateTime = DateTime.UtcNow;
            Old.Cost = New.Cost;

            Old.Date = New.Date;
            Old.Info = New.Info;
            
            Old.AccountId = New.AccountId;
            Old.UserId = New.UserId;
            Old.ProviderId = New.ProviderId;
            Old.ExchangeId = New.ExchangeId;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Deal> GetExist(IEnumerable<Deal> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Deals.Where(x => ids.Contains(x.Id));
    }

    public override Task RunPostProcessAsync(RepositoryActions action, Deal entity)
    {
        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Portfolio, QueueEntities.Deal, RabbitHelper.GetAction(action), entity);
        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Deal> entities)
    {
        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Portfolio, QueueEntities.Deals, RabbitHelper.GetAction(action), entities);
        return Task.CompletedTask;
    }
}