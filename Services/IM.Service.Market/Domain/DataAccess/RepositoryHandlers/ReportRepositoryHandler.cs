using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Market.Enums;


namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class ReportRepositoryHandler : RepositoryHandler<Report, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public ReportRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Report>> RunUpdateRangeHandlerAsync(IEnumerable<Report> entities)
    {
        entities = entities.ToArray();
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
    public override async Task<IEnumerable<Report>> RunDeleteRangeHandlerAsync(IEnumerable<Report> entities)
    {
        var comparer = new DataQuarterComparer<Report>();
        var result = new List<Report>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Reports.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public override Task RunPostProcessAsync(RepositoryActions action, Report entity)
    {
        RabbitPublisher publisher;

        if (action is RepositoryActions.Delete)
        {
            publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Report, QueueActions.Compute, entity);
            return Task.CompletedTask;
        }

        switch (entity.StatusId)
        {
            case (byte) Statuses.New:
                publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
                publisher.PublishTask(QueueNames.MarketData, QueueEntities.Report, QueueActions.Set, entity);
                publisher.PublishTask(QueueNames.MarketData, QueueEntities.Report, RabbitHelper.GetQueueAction(action), entity);
                return Task.CompletedTask;
            default:
                return Task.CompletedTask;
        }
    }
    public override Task RunPostProcessAsync(RepositoryActions action, IReadOnlyCollection<Report> entities)
    {
        RabbitPublisher publisher;

        if (action is RepositoryActions.Delete)
        {
            publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Reports, QueueActions.Compute, entities);
            return Task.CompletedTask;
        }

        if (!entities.Any(x => x.StatusId is (byte)Statuses.New))
            return Task.CompletedTask;

        publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Reports, QueueActions.Set, entities.Where(x => x.StatusId is (byte)Statuses.New));
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Reports, RabbitHelper.GetQueueAction(action), entities.Where(x => x.StatusId is (byte)Statuses.New));

        return Task.CompletedTask;
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
}