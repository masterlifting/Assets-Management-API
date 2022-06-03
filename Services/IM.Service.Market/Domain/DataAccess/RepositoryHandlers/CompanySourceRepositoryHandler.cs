using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.RepositoryService;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class CompanySourceRepositoryHandler : RepositoryHandler<CompanySource>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public CompanySourceRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
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

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.Market, QueueEntities.CompanySource, QueueActions.Get, entity);

        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<CompanySource> entities)
    {
        if (action is RepositoryActions.Delete)
            return Task.CompletedTask;

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.Market, QueueEntities.CompanySources, QueueActions.Get, entities);

        return Task.CompletedTask;
    }
}