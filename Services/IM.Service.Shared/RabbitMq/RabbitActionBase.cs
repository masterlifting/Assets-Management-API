using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Shared.Helpers.LogHelper;

namespace IM.Service.Shared.RabbitMq;

public abstract class RabbitActionBase
{
    private readonly ILogger logger;
    private readonly string rabbitConnectionString;
    private readonly Dictionary<QueueExchanges, IRabbitAction> actions;
    protected RabbitActionBase(string rabbitConnectionString, ILogger logger, Dictionary<QueueExchanges, IRabbitAction> actions)
    {
        this.logger = logger;
        this.rabbitConnectionString = rabbitConnectionString;
        this.actions = actions;
    }

    public async Task<bool> StartAsync(QueueExchanges exchange, string routingKey, string data)
    {
        var route = routingKey.Split('.');

        try
        {
            if (route.Length < 3)
                throw new ArgumentOutOfRangeException(nameof(route.Length), "Queue route length invalid");
            if (!Enum.TryParse(route[1], true, out QueueEntities entity))
                throw new ArgumentException($"Queue entity '{route[1]}' not recognized");
            if (!Enum.TryParse(route[2], true, out QueueActions action))
                throw new ArgumentException($"Queue action '{route[2]}' not recognized");
            if (!actions.ContainsKey(exchange))
                throw new ArgumentException($"Exchange '{exchange}' not found");

            await actions[exchange].GetResultAsync(entity, action, data).ConfigureAwait(false);

            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(nameof(StartAsync), exception);
            return false;
        }
    }
    public void Publish<T>(QueueExchanges exchange, QueueNames queue, QueueEntities entity, QueueActions action, T data) where T : class
    {
        using var publisher = new RabbitPublisher(rabbitConnectionString, exchange);
        publisher.PublishTask(queue, entity, action, data);
    }
    public void Publish<T>(QueueExchanges exchange, IEnumerable<QueueNames> queues, QueueEntities entity, QueueActions action, T data) where T : class
    {
        using var publisher = new RabbitPublisher(rabbitConnectionString, exchange);
        publisher.PublishTask(queues, entity, action, data);
    }
    public void Publish(QueueExchanges exchange, IEnumerable<(QueueNames, QueueEntities, QueueActions, object)> taskParams)
    {
        using var publisher = new RabbitPublisher(rabbitConnectionString, exchange);
        
        foreach (var task in taskParams)
            publisher.PublishTask(task.Item1,task.Item2, task.Item3, task.Item4);
    }
}