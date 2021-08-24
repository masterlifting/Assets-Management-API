
using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.PriceServices;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations
{
    public class RabbitPriceService : IRabbitActionService
    {
        private readonly PriceLoader priceLoader;
        public RabbitPriceService(PriceLoader priceLoader) => this.priceLoader = priceLoader;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope)
        {
            if (entity == QueueEntities.price && action == QueueActions.download && RabbitHelper.TrySerialize(data, out Ticker ticker) && ticker is not null)
                await priceLoader.LoadPricesAsync(ticker);

            return true;
        }
    }
}
