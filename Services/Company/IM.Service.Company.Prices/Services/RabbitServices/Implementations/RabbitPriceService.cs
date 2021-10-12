using CommonServices.RabbitServices;

using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.Services.PriceServices;

using System.Threading.Tasks;

namespace IM.Service.Company.Prices.Services.RabbitServices.Implementations
{
    public class RabbitPriceService : IRabbitActionService
    {
        private readonly PriceLoader priceLoader;
        public RabbitPriceService(PriceLoader priceLoader) => this.priceLoader = priceLoader;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
        {
            if (entity == QueueEntities.Price && action == QueueActions.GetData && RabbitHelper.TrySerialize(data, out Ticker? ticker))
                await priceLoader.LoadAsync(ticker!);

            return true;
        }
    }
}
