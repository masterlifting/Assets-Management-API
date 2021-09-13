using CommonServices.RabbitServices;
using IM.Services.Company.Reports.DataAccess.Entities;
using IM.Services.Company.Reports.DataAccess.Repository;
using IM.Services.Company.Reports.Services.RabbitServices.Implementations;
using IM.Services.Company.Reports.Services.ReportServices;
using IM.Services.Company.Reports.Settings;
using Microsoft.Extensions.Options;

namespace IM.Services.Company.Reports.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options, ReportLoader reportLoader, RepositorySet<Ticker> tickerRepository) : base(
            new()
            {
                { QueueExchanges.Crud, new RabbitCrudService(options.Value.ConnectionStrings.Mq, tickerRepository) },
                { QueueExchanges.Loader, new RabbitReportService(reportLoader, options.Value.ConnectionStrings.Mq) }
            })
        { }
    }
}
