
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonServices.RabbitServices
{
    public class RabbitService
    {
        private readonly Dictionary<QueueExchanges, IRabbitActionService> actions;
        protected RabbitService(Dictionary<QueueExchanges, IRabbitActionService> actions) => this.actions = actions;

        public async Task<bool> GetActionResultAsync(QueueExchanges exchange, string routingKey, string data)
        {
            var route = routingKey.Split('.');

            return route.Length >= 3
                        && Enum.TryParse(route[1], true, out QueueEntities entity)
                        && Enum.TryParse(route[2], true, out QueueActions action)
                        && actions.ContainsKey(exchange)
                        && await actions[exchange].GetActionResultAsync(entity, action, data);
        }
    }
}
