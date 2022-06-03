using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Settings;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.RepositoryService;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class CompanyRepositoryHandler : RepositoryHandler<Company>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public CompanyRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Company>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Company> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => x.Id,
                y => y.Id,
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
            Old.IndustryId = New.IndustryId;
            Old.CountryId = New.CountryId;
            Old.Description = New.Description;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Company> GetExist(IEnumerable<Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key)
            .ToArray();

        return context.Companies.Where(x => existData.Contains(x.Id));
    }

    public override Task RunPostProcessAsync(RepositoryActions action, Company entity)
    {
        if (action is RepositoryActions.Delete)
            new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function)
                .PublishTask(QueueNames.Market, QueueEntities.Rating, QueueActions.Compute, new Rating());

        new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync).PublishTask(
            new[] { QueueNames.Recommendations, QueueNames.Portfolio },
            QueueEntities.Company,
            RabbitHelper.GetQueueAction(action),
            new CompanyMqDto(entity.Id, entity.CountryId, entity.Name));

        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Company> entities)
    {
        if (action is RepositoryActions.Delete)
            new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function)
                .PublishTask(QueueNames.Market, QueueEntities.Ratings, QueueActions.Compute, Array.Empty<Rating>());

        new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync).PublishTask(
            new[] { QueueNames.Recommendations, QueueNames.Portfolio },
            QueueEntities.Companies,
            RabbitHelper.GetQueueAction(action),
            entities.Select(x => new CompanyMqDto(x.Id, x.CountryId, x.Name)));

        return Task.CompletedTask;
    }
}