using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;
using IM.Services.Analyzer.Api.Services.RabbitServices.Implementations;

namespace IM.Services.Analyzer.Api.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(
            AnalyzerRepository<Ticker> tickerRepository,
            AnalyzerRepository<Report> reportRepository,
            AnalyzerRepository<Price> priceRepository) : base(
            new()
            {
                { QueueExchanges.crud, new RabbitCrudService(tickerRepository) },
                { QueueExchanges.calculator, new RabbitCalculatorService(reportRepository, priceRepository) }
            })
        { }
    }
}
