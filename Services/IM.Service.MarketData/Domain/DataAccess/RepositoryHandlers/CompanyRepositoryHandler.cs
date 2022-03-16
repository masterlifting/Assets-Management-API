using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.MarketData.Domain.DataAccess.Comparators;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Models.Api.Amqp;
using IM.Service.MarketData.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.MarketData.Domain.DataAccess.RepositoryHandlers;

public class CompanyRepositoryHandler : RepositoryHandler<Company, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public CompanyRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Company>> GetUpdateRangeHandlerAsync(IEnumerable<Company> entities)
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
    public override async Task<IEnumerable<Company>> GetDeleteRangeHandlerAsync(IEnumerable<Company> entities)
    {
        var comparer = new CompanyComparer();
        var ctxEntities = await context.Companies.ToArrayAsync();
        return ctxEntities.Except(entities, comparer);
    }

    public override Task SetPostProcessAsync(RepositoryActions action, Company entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync);

        publisher.PublishTask(
            new[] { QueueNames.Recommendation, QueueNames.PortfolioData },
            QueueEntities.Company,
            RabbitHelper.GetQueueAction(action),
            new CompanyDto(entity.Id, entity.CountryId, entity.Name));

        return Task.CompletedTask;
    }
    public override Task SetPostProcessAsync(RepositoryActions action, IReadOnlyCollection<Company> entities)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync);
        publisher.PublishTask(
            new[] { QueueNames.Recommendation, QueueNames.PortfolioData },
            QueueEntities.Companies,
            RabbitHelper.GetQueueAction(action),
            entities.Select(x => new CompanyDto(x.Id, x.CountryId, x.Name)));

        return Task.CompletedTask;
    }

    public override IQueryable<Company> GetExist(IEnumerable<Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key)
            .ToArray();

        return context.Companies.Where(x => existData.Contains(x.Id));
    }
}