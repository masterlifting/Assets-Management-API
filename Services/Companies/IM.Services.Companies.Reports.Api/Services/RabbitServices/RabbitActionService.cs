using CommonServices.RabbitServices;

using IM.Services.Companies.Reports.Api.Services.RabbitServices.Implementations;
using IM.Services.Companies.Reports.Api.Services.ReportServices;
using IM.Services.Companies.Reports.Api.Settings;

using Microsoft.Extensions.Options;

namespace IM.Services.Companies.Reports.Api.Services.RabbitServices
{
    public class RabbitActionService : RabbitService
    {
        public RabbitActionService(IOptions<ServiceSettings> options, ReportLoader reportLoader) : base(
            new()
            {
                { QueueExchanges.crud, new RabbitCrudService(options.Value.ConnectionStrings.Mq) },
                { QueueExchanges.loader, new RabbitReportService(reportLoader) }
            })
        { }
    }
}
