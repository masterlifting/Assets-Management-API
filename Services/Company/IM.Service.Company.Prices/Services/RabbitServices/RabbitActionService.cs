using CommonServices.RabbitServices;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.DataAccess.Repository;
using IM.Service.Company.Prices.Services.PriceServices;
using IM.Service.Company.Prices.Services.RabbitServices.Implementations;
using IM.Service.Company.Prices.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Prices.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options, PriceLoader priceLoader, RepositorySet<Ticker> tickerRepository) : base(
            new()
            {
                { QueueExchanges.Crud, new RabbitCrudService(options.Value.ConnectionStrings.Mq, tickerRepository) },
                { QueueExchanges.Data, new RabbitPriceService(priceLoader) }
            })
        { }
    }
}
