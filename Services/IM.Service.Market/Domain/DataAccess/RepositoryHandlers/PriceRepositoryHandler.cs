using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class PriceRepositoryHandler : RepositoryHandler<Price, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public PriceRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Price>> RunUpdateRangeHandlerAsync(IEnumerable<Price> entities)
    {
        entities = entities.ToArray();
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
        RabbitPublisher publisher;

        if (action is RepositoryActions.Delete)
        {
            var lastEntity = await context.Prices.Where(x =>
                x.CompanyId == entity.CompanyId
                && x.SourceId == entity.SourceId
                && x.CurrencyId == entity.CurrencyId
                && x.Date < entity.Date)
                .OrderBy(x => x.Date)
                .LastOrDefaultAsync();

            publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

            if (lastEntity is not null)
                publisher.PublishTask(QueueNames.MarketData, QueueEntities.Price, QueueActions.Set, lastEntity);

            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Price, QueueActions.Delete, entity);
        }
        else if (entity.StatusId is (byte)Statuses.New)
        {
            publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Price, RabbitHelper.GetQueueAction(action), entity);
        }
    }
    public override async Task RunPostProcessAsync(RepositoryActions action, IReadOnlyCollection<Price> entities)
    {
        RabbitPublisher publisher;

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

            publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(QueueNames.MarketData, QueueEntities.Prices, QueueActions.Delete, entities);

            if (lastEntities.Any())
                publisher.PublishTask(QueueNames.MarketData, QueueEntities.Prices, QueueActions.Set, lastEntities);
        }

        var newEntities = entities.Where(x => x.StatusId is (byte)Statuses.New).ToArray();

        if (!newEntities.Any())
            return;

        publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Prices, RabbitHelper.GetQueueAction(action), newEntities);
    }
}