using CommonServices.RabbitServices;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.DataAccess.Repository;
using IM.Service.Company.Reports.Services.RabbitServices.Implementations;
using IM.Service.Company.Reports.Services.ReportServices;
using IM.Service.Company.Reports.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Reports.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options, ReportLoader reportLoader, RepositorySet<Ticker> tickerRepository) : base(
            new()
            {
                { QueueExchanges.Crud, new RabbitCrudService(options.Value.ConnectionStrings.Mq, tickerRepository) },
                { QueueExchanges.Data, new RabbitReportService(reportLoader, options.Value.ConnectionStrings.Mq) }
            })
        { }
    }
}
