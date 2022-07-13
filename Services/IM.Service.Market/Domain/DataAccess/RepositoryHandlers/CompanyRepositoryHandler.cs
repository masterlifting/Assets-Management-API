using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class CompanyRepositoryHandler : RepositoryHandler<Company>
{
    private readonly DatabaseContext context;
    private readonly RabbitAction rabbitAction;
    public CompanyRepositoryHandler(RabbitAction rabbitAction, DatabaseContext context)
    {
        this.context = context;
        this.rabbitAction = rabbitAction;
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
            rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.Rating, QueueActions.Compute, new Rating());

        rabbitAction.Publish(QueueExchanges.Sync,
            new[] { QueueNames.Recommendations, QueueNames.Portfolio },
            QueueEntities.Asset,
            RabbitHelper.GetAction(action),
            new AssetMqDto(entity.Id, (byte)AssetTypes.Stock, entity.CountryId, entity.Name));

        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Company> entities)
    {
        if (action is RepositoryActions.Delete)
            rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.Ratings, QueueActions.Compute, Array.Empty<Rating>());

        rabbitAction.Publish(QueueExchanges.Sync,
            new[] { QueueNames.Recommendations, QueueNames.Portfolio },
            QueueEntities.Assets,
            RabbitHelper.GetAction(action),
            entities.Select(x => new AssetMqDto(x.Id, (byte)AssetTypes.Stock, x.CountryId, x.Name)));

        return Task.CompletedTask;
    }
}