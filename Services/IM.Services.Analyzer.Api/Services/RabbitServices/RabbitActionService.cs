using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Services.RabbitServices.Implementations;
using IM.Services.Analyzer.Api.Settings;

using Microsoft.Extensions.Options;

namespace IM.Services.Analyzer.Api.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(
            IOptions<ServiceSettings> options,
            EntityRepository<Ticker, AnalyzerContext> tickerRepository,
            EntityRepository<Report, AnalyzerContext> reportRepository,
            EntityRepository<Price, AnalyzerContext> priceRepository) : base(
            new()
            {
                { QueueExchanges.crud, new RabbitCrudService(tickerRepository) },
                { QueueExchanges.calculator, new RabbitCalculatorService(reportRepository, priceRepository) }
            })
        { }
    }
}
