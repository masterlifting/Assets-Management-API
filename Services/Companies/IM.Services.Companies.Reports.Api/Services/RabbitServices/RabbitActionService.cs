using CommonServices.RabbitServices;
using CommonServices.RepositoryService;

using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.Services.RabbitServices.Implementations;
using IM.Services.Companies.Reports.Api.Services.ReportServices;
using IM.Services.Companies.Reports.Api.Settings;

using Microsoft.Extensions.Options;

namespace IM.Services.Companies.Reports.Api.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options, ReportLoader reportLoader, EntityRepository<Ticker, ReportsContext> crudRepository) : base(
            new()
            {
                { QueueExchanges.crud, new RabbitCrudService(options.Value.ConnectionStrings.Mq, crudRepository) },
                { QueueExchanges.loader, new RabbitReportService(reportLoader, options.Value.ConnectionStrings.Mq) }
            })
        { }
    }
}
