using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Amqp;
using IM.Service.Market.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class CompanyRepositoryHandler : RepositoryHandler<Company, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public CompanyRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Company>> RunUpdateRangeHandlerAsync(IEnumerable<Company> entities)
    {
        entities = entities.ToArray();
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
                .PublishTask(QueueNames.MarketData, QueueEntities.Rating, QueueActions.Compute, string.Empty);

        new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync).PublishTask(
            new[] { QueueNames.Recommendation, QueueNames.Portfolio },
            QueueEntities.Company,
            RabbitHelper.GetQueueAction(action),
            new CompanyDto(entity.Id, entity.CountryId, entity.Name));

        return Task.CompletedTask;
    }
    public override Task RunPostProcessAsync(RepositoryActions action, IReadOnlyCollection<Company> entities)
    {

        if (action is RepositoryActions.Delete)
            new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function)
                .PublishTask(QueueNames.MarketData, QueueEntities.Ratings, QueueActions.Compute, string.Empty);

        new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync).PublishTask(
            new[] { QueueNames.Recommendation, QueueNames.Portfolio },
            QueueEntities.Companies,
            RabbitHelper.GetQueueAction(action),
            entities.Select(x => new CompanyDto(x.Id, x.CountryId, x.Name)));

        return Task.CompletedTask;
    }
}