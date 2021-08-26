using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonServices.RabbitServices
{
    public class RabbitService
    {
        private readonly Dictionary<QueueExchanges, IRabbitActionService> actions;
        public RabbitService(Dictionary<QueueExchanges, IRabbitActionService> actions) => this.actions = actions is null ? new() : actions;

        public virtual async Task<bool> GetActionResultAsync(QueueExchanges exchange, string routingKey, string data, IServiceScope scope)
        {
            var route = routingKey.Split('.');

            return route.Length >= 3
                        && Enum.TryParse(route[1].ToLowerInvariant().Trim(), out QueueEntities entity)
                        && Enum.TryParse(route[2].ToLowerInvariant().Trim(), out QueueActions action)
                        && actions.ContainsKey(exchange)
                        && await actions[exchange].GetActionResultAsync(entity, action, data, scope);
        }
    }
}
