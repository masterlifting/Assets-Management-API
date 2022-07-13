using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class SplitRepositoryHandler : RepositoryHandler<Split>
{
    private readonly DatabaseContext context;
    private readonly RabbitAction rabbitAction;
    public SplitRepositoryHandler(RabbitAction rabbitAction, DatabaseContext context)
    {
        this.context = context;
        this.rabbitAction = rabbitAction;
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
        var queueTaskParams = new List<(QueueNames, QueueEntities, QueueActions, object)>(2);

        if (action is RepositoryActions.Create)
        {
            var companySource = context.CompanySources.Find(entity.CompanyId, entity.SourceId);
            
            if(companySource is not null)
                queueTaskParams.Add((QueueNames.Market, QueueEntities.AssetSource, QueueActions.Get, companySource));
        }

        queueTaskParams.Add((QueueNames.Market, QueueEntities.Split, RabbitHelper.GetAction(action), entity));
        
        rabbitAction.Publish(QueueExchanges.Function, queueTaskParams);

        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Split> entities)
    {
        var queueTaskParams = new List<(QueueNames, QueueEntities, QueueActions, object)>(2);

        if (action is RepositoryActions.Create)
        {
            var companyIds = entities.Select(x => x.CompanyId).Distinct();
            var sourceIds = entities.Select(x => x.SourceId).Distinct();
                
            var companySources = context.CompanySources
                .Where(x => 
                    companyIds.Contains(x.CompanyId) 
                    && sourceIds.Contains(x.SourceId))
                .ToArray();


            queueTaskParams.Add((QueueNames.Market, QueueEntities.AssetSources, QueueActions.Get, companySources));
        }

        queueTaskParams.Add((QueueNames.Market, QueueEntities.Splits, RabbitHelper.GetAction(action), entities));

        rabbitAction.Publish(QueueExchanges.Function, queueTaskParams);

        return Task.CompletedTask;
    }
}