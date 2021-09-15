using System.Collections.Generic;
using CommonServices.RabbitServices;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.RabbitServices.Implementations;

namespace IM.Service.Company.Analyzer.Services.RabbitServices
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
                { QueueExchanges.Logic, new RabbitCalculatorService(reportRepository, priceRepository) }
            })
        { }
    }
}
