using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class DealRepositoryHandler : RepositoryHandler<Deal>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public DealRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }


    public override async Task<IEnumerable<Deal>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Deal> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Cost = New.CurrencyId;
            Old.Value = New.Value;
            Old.Info = New.Info;
            Old.DerivativeId = New.DerivativeId;
            Old.DerivativeCode = New.DerivativeCode;
            Old.ExchangeId = New.ExchangeId;
            Old.AccountBrokerId = New.AccountBrokerId;
            Old.AccountUserId = New.AccountUserId;
            Old.AccountName = New.AccountName;
            Old.OperationId = New.OperationId;
            Old.CurrencyId = New.CurrencyId;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Deal> GetExist(IEnumerable<Deal> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Deals.Where(x => ids.Contains(x.Id));
    }

    public override Task RunPostProcessAsync(RepositoryActions action, Deal entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.Portfolio, QueueEntities.Deal, RabbitHelper.GetQueueAction(action), entity);
        return Task.CompletedTask;
    }
    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Deal> entities)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.Portfolio, QueueEntities.Deals, RabbitHelper.GetQueueAction(action), entities);
        return Task.CompletedTask;
    }
}