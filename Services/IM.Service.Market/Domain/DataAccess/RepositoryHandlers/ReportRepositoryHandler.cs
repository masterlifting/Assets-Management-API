using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.RepositoryService;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Shared.Enums;
using static IM.Service.Market.Enums;


namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class ReportRepositoryHandler : RepositoryHandler<Report>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public ReportRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
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
        RabbitPublisher publisher;

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

            publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

            if (lastEntity is not null)
                publisher.PublishTask(QueueNames.Market, QueueEntities.Report, QueueActions.Set, lastEntity);

            publisher.PublishTask(QueueNames.Market, QueueEntities.Report, QueueActions.Delete, entity);
        }
        else if (entity.StatusId is (byte)Statuses.New)
        {
            publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(QueueNames.Market, QueueEntities.Report, QueueActions.Set, entity);
            publisher.PublishTask(QueueNames.Market, QueueEntities.Report, RabbitHelper.GetQueueAction(action), entity);
        }
    }
    public override async Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Report> entities)
    {
        RabbitPublisher publisher;

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

            publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(QueueNames.Market, QueueEntities.Reports, QueueActions.Delete, entities);

            if (lastEntities.Any())
                publisher.PublishTask(QueueNames.Market, QueueEntities.Reports, QueueActions.Set, lastEntities);
        }

        var newEntities = entities.Where(x => x.StatusId is (byte)Statuses.New).ToArray();

        if (!newEntities.Any())
            return;

        publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.Market, QueueEntities.Reports, QueueActions.Set, newEntities);
        publisher.PublishTask(QueueNames.Market, QueueEntities.Reports, RabbitHelper.GetQueueAction(action), newEntities);
    }
}