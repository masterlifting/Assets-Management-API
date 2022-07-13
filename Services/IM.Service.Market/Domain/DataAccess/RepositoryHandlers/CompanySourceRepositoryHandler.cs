using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class CompanySourceRepositoryHandler : RepositoryHandler<CompanySource>
{
    private readonly DatabaseContext context;
    private readonly RabbitAction rabbitAction;

    public CompanySourceRepositoryHandler(RabbitAction rabbitAction, DatabaseContext context)
    {
        this.context = context;
        this.rabbitAction = rabbitAction;
    }

    public override async Task<IEnumerable<CompanySource>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<CompanySource> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.SourceId),
                y => (y.CompanyId, y.SourceId),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Value = New.Value;

        return result.Select(x => x.Old);
    }
    public override IQueryable<CompanySource> GetExist(IEnumerable<CompanySource> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var sourceIds = entities
            .GroupBy(x => x.SourceId)
            .Select(x => x.Key);

        return context.CompanySources.Where(x => companyIds.Contains(x.CompanyId) && sourceIds.Contains(x.SourceId));
    }

    public override Task RunPostProcessAsync(RepositoryActions action, CompanySource entity)
    {
        if (entity.Value is null || action is RepositoryActions.Delete)
            return Task.CompletedTask;

        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.AssetSource, QueueActions.Get, entity);

        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<CompanySource> entities)
    {
        if (action is RepositoryActions.Delete)
            return Task.CompletedTask;

        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.AssetSources, QueueActions.Get, entities);

        return Task.CompletedTask;
    }
}