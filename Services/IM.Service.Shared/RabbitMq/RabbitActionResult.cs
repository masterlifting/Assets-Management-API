using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Shared.Helpers.LogHelper;

namespace IM.Service.Shared.RabbitMq;

public abstract class RabbitActionResult
{
    private readonly ILogger logger;
    private readonly Dictionary<QueueExchanges, IRabbitAction> actions;
    protected RabbitActionResult(ILogger logger, Dictionary<QueueExchanges, IRabbitAction> actions)
    {
        this.logger = logger;
        this.actions = actions;
    }

    public async Task<bool> GetResultAsync(QueueExchanges exchange, string routingKey, string data)
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
            logger.LogError(nameof(GetResultAsync), exception);
            return false;
        }
    }
}