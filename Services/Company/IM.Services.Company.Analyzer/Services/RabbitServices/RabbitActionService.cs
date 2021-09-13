using System.Collections.Generic;
using CommonServices.RabbitServices;
using IM.Services.Company.Analyzer.DataAccess.Entities;
using IM.Services.Company.Analyzer.DataAccess.Repository;
using IM.Services.Company.Analyzer.Services.RabbitServices.Implementations;

namespace IM.Services.Company.Analyzer.Services.RabbitServices
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
