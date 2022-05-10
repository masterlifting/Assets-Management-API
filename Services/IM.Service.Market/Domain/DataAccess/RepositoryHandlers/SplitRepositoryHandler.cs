using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class SplitRepositoryHandler : RepositoryHandler<Split>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public SplitRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Split>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Split> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.SourceId, x.Date),
                y => (y.CompanyId, y.SourceId, y.Date),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Value = New.Value;

        return result.Select(x => x.Old).ToArray();
    }
    public override IQueryable<Split> GetExist(IEnumerable<Split> entities)
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

        return context.Splits
            .Where(x => 
                companyIds.Contains(x.CompanyId) 
                && sourceIds.Contains(x.SourceId) 
                && dates.Contains(x.Date));
    }

    public override Task RunPostProcessAsync(RepositoryActions action, Split entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        if (action is RepositoryActions.Create)
        {
            var companySource = context.CompanySources.Find(entity.CompanyId, entity.SourceId);
            
            if(companySource is not null)
                publisher.PublishTask(QueueNames.Market, QueueEntities.Float, QueueActions.Get, companySource);
        }

        publisher.PublishTask(QueueNames.Market, QueueEntities.Split, RabbitHelper.GetQueueAction(action), entity);

        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Split> entities)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        if (action is RepositoryActions.Create)
        {
            var companyIds = entities.Select(x => x.CompanyId).Distinct();
            var sourceIds = entities.Select(x => x.SourceId).Distinct();
                
            var companySources = context.CompanySources
                .Where(x => 
                    companyIds.Contains(x.CompanyId) 
                    && sourceIds.Contains(x.SourceId))
                .ToArray();

            publisher.PublishTask(QueueNames.Market, QueueEntities.Floats, QueueActions.Get, companySources);
        }

        publisher.PublishTask(QueueNames.Market, QueueEntities.Splits, RabbitHelper.GetQueueAction(action), entities);

        return Task.CompletedTask;
    }
}