using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using static IM.Service.Market.Enums;
using static IM.Service.Shared.Enums;


namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class ReportRepositoryHandler : RepositoryHandler<Report>
{
    private readonly DatabaseContext context;
    private readonly RabbitAction rabbitAction;
    public ReportRepositoryHandler(RabbitAction rabbitAction, DatabaseContext context)
    {
        this.context = context;
        this.rabbitAction = rabbitAction;
    }

    public override async Task<IEnumerable<Report>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Report> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.SourceId, x.Year, x.Quarter),
                y => (y.CompanyId, y.SourceId, y.Year, y.Quarter),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Multiplier = New.Multiplier;
            Old.CurrencyId = New.CurrencyId;

            Old.Turnover = New.Turnover;
            Old.LongTermDebt = New.LongTermDebt;
            Old.Asset = New.Asset;
            Old.CashFlow = New.CashFlow;
            Old.Obligation = New.Obligation;
            Old.ProfitGross = New.ProfitGross;
            Old.ProfitNet = New.ProfitNet;
            Old.Revenue = New.Revenue;
            Old.ShareCapital = New.ShareCapital;

            Old.StatusId = New.StatusId;
            Old.Result = New.Result;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Report> GetExist(IEnumerable<Report> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var sourceIds = entities
            .GroupBy(x => x.SourceId)
            .Select(x => x.Key);
        var years = entities
            .GroupBy(x => x.Year)
            .Select(x => x.Key);
        var quarters = entities
            .GroupBy(x => x.Quarter)
            .Select(x => x.Key);

        return context.Reports
            .Where(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId)
                && years.Contains(x.Year)
                && quarters.Contains(x.Quarter));
    }

    public override async Task RunPostProcessAsync(RepositoryActions action, Report entity)
    {
        var queueTaskParams = new List<(QueueNames, QueueEntities, QueueActions, object)>(3);

        if (action is RepositoryActions.Delete)
        {
            var lastEntity = await context.Reports
                .Where(x =>
                x.CompanyId == entity.CompanyId
                && x.SourceId == entity.SourceId
                && x.CurrencyId == entity.CurrencyId
                && x.Year < entity.Year || x.Year == entity.Year && x.Quarter < entity.Quarter)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Quarter)
                .LastOrDefaultAsync();

            if (lastEntity is not null)
                queueTaskParams.Add((QueueNames.Market, QueueEntities.Report, QueueActions.Set, lastEntity));

            queueTaskParams.Add((QueueNames.Market, QueueEntities.Report, QueueActions.Delete, entity));
        }
        else if (entity.StatusId is (byte)Statuses.New)
        {
            queueTaskParams.Add((QueueNames.Market, QueueEntities.Report, QueueActions.Set, entity));
            queueTaskParams.Add((QueueNames.Market, QueueEntities.Report, RabbitHelper.GetAction(action), entity));
        }

        rabbitAction.Publish(QueueExchanges.Transfer, queueTaskParams);
    }
    public override async Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Report> entities)
    {
        var queueTaskParams = new List<(QueueNames, QueueEntities, QueueActions, object)>(3);

        if (action is RepositoryActions.Delete)
        {
            var lastEntities = new List<Report>(entities.Count);
            foreach (var group in entities.GroupBy(x => (x.CompanyId, x.SourceId, x.CurrencyId)))
            {
                var minEntity = group.MinBy(x => (x.Year, x.Quarter))!;

                var lastEntity = await context.Reports
                    .Where(x =>
                    x.CompanyId == group.Key.CompanyId
                    && x.SourceId == group.Key.SourceId
                    && x.CurrencyId == group.Key.CurrencyId
                    && x.Year < minEntity.Year || x.Year == minEntity.Year && x.Quarter < minEntity.Quarter)
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Quarter)
                    .LastOrDefaultAsync();

                if (lastEntity is not null)
                    lastEntities.Add(lastEntity);
            }

            queueTaskParams.Add((QueueNames.Market, QueueEntities.Reports, QueueActions.Delete, entities));

            if (lastEntities.Any())
                queueTaskParams.Add((QueueNames.Market, QueueEntities.Reports, QueueActions.Set, lastEntities));
        }

        var newEntities = entities.Where(x => x.StatusId is (byte)Statuses.New).ToArray();

        if (!newEntities.Any())
            return;

        queueTaskParams.Add((QueueNames.Market, QueueEntities.Reports, QueueActions.Set, newEntities));
        queueTaskParams.Add((QueueNames.Market, QueueEntities.Reports, RabbitHelper.GetAction(action), newEntities));
        rabbitAction.Publish(QueueExchanges.Transfer, queueTaskParams);
    }
}