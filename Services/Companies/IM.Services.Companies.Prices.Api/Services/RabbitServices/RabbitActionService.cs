using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.Services.PriceServices;
using IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices
{
    public class RabbitActionService
    {
        private readonly Dictionary<QueueExchanges, IRabbitActionService> actions;
        public RabbitActionService(RabbitService rabbitService, PriceLoader priceLoader)
        {
            actions = new()
            {
                { QueueExchanges.crud, new RabbitCrudService(rabbitService) },
                { QueueExchanges.loader, new RabbitPriceService(priceLoader) }
            };
        }

        public async Task<bool> GetActionResultAsync(QueueExchanges exchange, string routingKey, string data, IServiceScope scope)
        {
            var route = routingKey.Split('.');

            return route.Length >= 3
                        && Enum.TryParse(route[1].ToLowerInvariant().Trim(), out QueueEntities entity)
                        && Enum.TryParse(route[2].ToLowerInvariant().Trim(), out QueueActions action)
                        && await actions[exchange].GetActionResultAsync(entity, action, data, scope);
        }
    }
}
