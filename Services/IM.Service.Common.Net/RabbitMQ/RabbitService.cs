using IM.Service.Common.Net.RabbitMQ.Configuration;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Helpers.LogHelper;
namespace IM.Service.Common.Net.RabbitMQ;

public class RabbitService
{
    private readonly ILogger<RabbitService> logger;
    private readonly Dictionary<QueueExchanges, IRabbitActionService> actions;
    protected RabbitService(ILogger<RabbitService> logger, Dictionary<QueueExchanges, IRabbitActionService> actions)
    {
        this.logger = logger;
        this.actions = actions;
    }

    public async Task<bool> GetActionResultAsync(QueueExchanges exchange, string routingKey, string data)
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

            await actions[exchange].GetActionResultAsync(entity, action, data).ConfigureAwait(false);

            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(nameof(GetActionResultAsync), exception);
            return false;
        }
    }
}