using CommonServices.RabbitServices;
using IM.Services.Company.Prices.DataAccess.Entities;
using IM.Services.Company.Prices.DataAccess.Repository;
using IM.Services.Company.Prices.Services.PriceServices;
using IM.Services.Company.Prices.Services.RabbitServices.Implementations;
using IM.Services.Company.Prices.Settings;
using Microsoft.Extensions.Options;

namespace IM.Services.Company.Prices.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options, PriceLoader priceLoader, RepositorySet<Ticker> tickerRepository) : base(
            new()
            {
                { QueueExchanges.Crud, new RabbitCrudService(options.Value.ConnectionStrings.Mq, tickerRepository) },
                { QueueExchanges.Loader, new RabbitPriceService(priceLoader, options.Value.ConnectionStrings.Mq) }
            })
        { }
    }
}
