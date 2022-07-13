using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using System.Data;

using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class FloatRepositoryHandler : RepositoryHandler<Float>
{
    private readonly DatabaseContext context;
    private readonly RabbitAction rabbitAction;
    public FloatRepositoryHandler(RabbitAction rabbitAction, DatabaseContext context)
    {
        this.context = context;
        this.rabbitAction = rabbitAction;
    }
    public override async Task<Float> RunCreateHandlerAsync(Float entity)
    {
        var existEntities = await GetExist(new[] { entity }).ToArrayAsync();

        return existEntities.Any()
            ? throw new ConstraintException($"{nameof(entity.Value)}: '{entity.Value}' for '{entity.CompanyId}' is already.")
            : entity;
    }

    public override async Task<IEnumerable<Float>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Float> entities)
    {
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

    public override Task RunPostProcessAsync(RepositoryActions action, Float entity)
    {
        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.Float, RabbitHelper.GetAction(action), entity);
        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Float> entities)
    {
        rabbitAction.Publish(QueueExchanges.Function, QueueNames.Market, QueueEntities.Floats, RabbitHelper.GetAction(action), entities);
        return Task.CompletedTask;
    }
}