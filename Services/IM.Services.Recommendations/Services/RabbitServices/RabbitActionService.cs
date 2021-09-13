using System.Collections.Generic;
using CommonServices.RabbitServices;

using IM.Services.Recommendations.DataAccess.Entities;
using IM.Services.Recommendations.DataAccess.Repository;
using IM.Services.Recommendations.Services.RabbitServices.Implementations;

namespace IM.Services.Recommendations.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(
            RepositorySet<Ticker> tickerRepository,
            RepositorySet<Report> reportRepository,
            RepositorySet<Price> priceRepository) : base(
            new Dictionary<QueueExchanges, IRabbitActionService>
            {
                { QueueExchanges.Crud, new RabbitCrudService(tickerRepository) },
                { QueueExchanges.Calculator, new RabbitCalculatorService(reportRepository, priceRepository) }
            })
        { }
    }
}
