using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.Services.PriceServices;
using IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations;
using IM.Services.Companies.Prices.Api.Settings;

using Microsoft.Extensions.Options;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options, PriceLoader priceLoader) : base(
            new()
            {
                { QueueExchanges.crud, new RabbitCrudService(options.Value.ConnectionStrings.Mq) },
                { QueueExchanges.loader, new RabbitPriceService(priceLoader) }
            })
        { }
    }
}
