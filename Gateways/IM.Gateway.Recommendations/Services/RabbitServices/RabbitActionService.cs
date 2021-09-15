using System.Collections.Generic;
using CommonServices.RabbitServices;
using IM.Gateway.Recommendations.DataAccess.Entities;
using IM.Gateway.Recommendations.DataAccess.Repository;
using IM.Gateway.Recommendations.Services.RabbitServices.Implementations;

namespace IM.Gateway.Recommendations.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService( RepositorySet<Ticker> tickerRepository) : base(
            new Dictionary<QueueExchanges, IRabbitActionService>
            {
                { QueueExchanges.Crud, new RabbitCrudService(tickerRepository) },
            })
        { }
    }
}
