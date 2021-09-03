using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.PriceServices;
using IM.Services.Companies.Prices.Api.Services.RabbitServices.Implementations;
using IM.Services.Companies.Prices.Api.Settings;

using Microsoft.Extensions.Options;

namespace IM.Services.Companies.Prices.Api.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options, PriceLoader priceLoader, EntityRepository<Ticker, PricesContext> tickerRepository) : base(
            new()
            {
                { QueueExchanges.crud, new RabbitCrudService(options.Value.ConnectionStrings.Mq, tickerRepository) },
                { QueueExchanges.loader, new RabbitPriceService(priceLoader, options.Value.ConnectionStrings.Mq) }
            })
        { }
    }
}
