using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using static IM.Service.Market.Enums;
using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class PriceRepositoryHandler : RepositoryHandler<Price>
{
    private readonly DatabaseContext context;
    private readonly RabbitAction rabbitAction;

    public PriceRepositoryHandler(RabbitAction rabbitAction, DatabaseContext context)
    {
        this.context = context;
        this.rabbitAction = rabbitAction;
    }

    public override async Task<IEnumerable<Price>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Price> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.SourceId, x.Date),
                y => (y.CompanyId, y.SourceId, y.Date),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.CurrencyId = New.CurrencyId;

            Old.Value = New.Value;
            Old.ValueTrue = New.ValueTrue;

            Old.StatusId = New.StatusId;
            Old.Result = New.Result;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Price> GetExist(IEnumerable<Price> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var sourceIds = entities
            .GroupBy(x => x.SourceId)
            .Select(x => x.Key);
        var dates = entities
            .GroupBy(x => x.Date)
            .Select(x => x.Key);

        return context.Prices
            .Where(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId)
                && dates.Contains(x.Date));
    }

    public override async Task RunPostProcessAsync(RepositoryActions action, Price entity)
    {
        var queueTaskParams = new List<(QueueNames, QueueEntities, QueueActions, object)>(3);

        if (action is RepositoryActions.Delete)
        {
            var lastEntity = await context.Prices.Where(x =>
                x.CompanyId == entity.CompanyId
                && x.SourceId == entity.SourceId
                && x.CurrencyId == entity.CurrencyId
                && x.Date < entity.Date)
                .OrderBy(x => x.Date)
                .LastOrDefaultAsync();

            if (lastEntity is not null)
                queueTaskParams.Add((QueueNames.Market, QueueEntities.Cost, QueueActions.Set, lastEntity));

            queueTaskParams.Add((QueueNames.Market, QueueEntities.Cost, QueueActions.Delete, entity));
        }
        else if (entity.StatusId is (byte)Statuses.New)
            queueTaskParams.Add((QueueNames.Market, QueueEntities.Cost, RabbitHelper.GetAction(action), entity));

        rabbitAction.Publish(QueueExchanges.Function, queueTaskParams);
    }
    public override async Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Price> entities)
    {
        var queueTaskParams = new List<(QueueNames, QueueEntities, QueueActions, object)>(3);

        if (action is RepositoryActions.Delete)
        {
            var lastEntities = new List<Price>(entities.Count);
            foreach (var group in entities.GroupBy(x => (x.CompanyId, x.SourceId, x.CurrencyId)))
            {
                var minEntity = group.MinBy(x => x.Date)!;

                var lastEntity = await context.Prices.Where(x =>
                    x.CompanyId == group.Key.CompanyId
                    && x.SourceId == group.Key.SourceId
                    && x.CurrencyId == group.Key.CurrencyId
                    && x.Date < minEntity.Date)
                    .OrderBy(x => x.Date)
                    .LastOrDefaultAsync();

                if (lastEntity is not null)
                    lastEntities.Add(lastEntity);
            }

            if (lastEntities.Any())
                queueTaskParams.Add((QueueNames.Market, QueueEntities.Costs, QueueActions.Set, lastEntities));

            queueTaskParams.Add((QueueNames.Market, QueueEntities.Costs, QueueActions.Delete, entities));
        }

        var newEntities = entities.Where(x => x.StatusId is (byte)Statuses.New).ToArray();

        if (!newEntities.Any())
            return;

        queueTaskParams.Add((QueueNames.Market, QueueEntities.Costs, RabbitHelper.GetAction(action), newEntities));

        rabbitAction.Publish(QueueExchanges.Function, queueTaskParams);
    }
}