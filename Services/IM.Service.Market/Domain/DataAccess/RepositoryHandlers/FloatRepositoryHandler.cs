using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Data;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Settings;
using static IM.Service.Common.Net.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class FloatRepositoryHandler : RepositoryHandler<Float, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public FloatRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<Float> GetCreateHandlerAsync(Float entity)
    {
        var existEntities = await GetExist(new[] { entity }).ToArrayAsync();

        return existEntities.Any()
            ? throw new ConstraintException($"{nameof(entity.Value)}: '{entity.Value}' for '{entity.CompanyId}' is already.")
            : entity;
    }
    public override async Task<IEnumerable<Float>> GetUpdateRangeHandlerAsync(IEnumerable<Float> entities)
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
            Old.Value = New.Value;
            Old.ValueFree = New.ValueFree;
        }

        return result.Select(x => x.Old).ToArray();
    }
    public override async Task<IEnumerable<Float>> GetDeleteRangeHandlerAsync(IEnumerable<Float> entities)
    {
        var comparer = new FloatComparer();
        var result = new List<Float>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Floats.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public override Task SetPostProcessAsync(RepositoryActions action, Float entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Float, RabbitHelper.GetQueueAction(action), entity);
        return Task.CompletedTask;
    }
    public override Task SetPostProcessAsync(RepositoryActions action, IReadOnlyCollection<Float> entities)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Floats, RabbitHelper.GetQueueAction(action), entities);
        return Task.CompletedTask;
    }

    public override IQueryable<Float> GetExist(IEnumerable<Float> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var sourceIds = entities
            .GroupBy(x => x.SourceId)
            .Select(x => x.Key);
        var values = entities
            .GroupBy(x => x.Value)
            .Select(x => x.Key);

        return context.Floats.Where(x => companyIds.Contains(x.CompanyId) && sourceIds.Contains(x.SourceId) && values.Contains(x.Value));
    }
}