
using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.Services.PriceServices;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

using static IM.Services.Companies.Prices.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations
{
    public class RabbitPriceService : IRabbitActionService
    {
        private readonly PriceLoader priceLoader;
        public RabbitPriceService(PriceLoader priceLoader) => this.priceLoader = priceLoader;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope)
        {
            if (entity == QueueEntities.price && action == QueueActions.download && Enum.TryParse(data, out PriceSourceTypes sourceType))
                await priceLoader.LoadPricesAsync(sourceType);

            return true;
        }
    }
}
