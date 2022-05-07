using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Common.Net.Enums;


namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class CoefficientRepositoryHandler : RepositoryHandler<Coefficient>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public CoefficientRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Coefficient>> RunUpdateRangeHandlerAsync(IEnumerable<Coefficient> entities)
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
            Old.Pe = New.Pe;
            Old.Pb = New.Pb;
            Old.DebtLoad = New.DebtLoad;
            Old.Profitability = New.Profitability;
            Old.Roa = New.Roa;
            Old.Roe = New.Roe;
            Old.Eps = New.Eps;

            Old.StatusId = New.StatusId;
            Old.Result = New.Result;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Coefficient> GetExist(IEnumerable<Coefficient> entities)
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

        return context.Coefficients
            .Where(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId)
                && years.Contains(x.Year)
                && quarters.Contains(x.Quarter));
    }

    public override async Task RunPostProcessAsync(RepositoryActions action, Coefficient entity)
    {
        if (action is not RepositoryActions.Delete)
            return;

        var lastEntity = await context.Coefficients.Where(x =>
            x.CompanyId == entity.CompanyId
            && x.SourceId == entity.SourceId
            && x.Year < entity.Year || x.Year == entity.Year && x.Quarter < entity.Quarter)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Quarter)
            .LastOrDefaultAsync();

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        if (lastEntity is not null)
            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Coefficient, QueueActions.Set, lastEntity);
        else
            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Rating, QueueActions.Compute, string.Empty);
    }
    public override async Task RunPostProcessAsync(RepositoryActions action, IReadOnlyCollection<Coefficient> entities)
    {
        if (action is not RepositoryActions.Delete)
            return;

        var lastEntities = new List<Coefficient>(entities.Count);
        foreach (var group in entities.GroupBy(x => (x.CompanyId, x.SourceId)))
        {
            var minEntity = group.MinBy(x => (x.Year, x.Quarter))!;

            var lastEntity = await context.Coefficients.Where(x =>
                x.CompanyId == group.Key.CompanyId
                && x.SourceId == group.Key.SourceId
                && x.Year < minEntity.Year || x.Year == minEntity.Year && x.Quarter < minEntity.Quarter)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Quarter)
                .LastOrDefaultAsync();

            if (lastEntity is not null)
                lastEntities.Add(lastEntity);
        }

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        if (lastEntities.Any())
            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Coefficients, QueueActions.Set, lastEntities);
        else
            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Ratings, QueueActions.Compute, string.Empty);
    }
}