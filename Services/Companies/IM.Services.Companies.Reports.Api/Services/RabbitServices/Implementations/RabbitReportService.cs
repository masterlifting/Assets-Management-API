
using CommonServices.RabbitServices;

using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.Services.ReportServices;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.RabbitServices.Implementations
{
    public class RabbitReportService : IRabbitActionService
    {
        private readonly ReportLoader reportLoader;
        public RabbitReportService(ReportLoader reportLoader) => this.reportLoader = reportLoader;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope)
        {
            if (entity == QueueEntities.report && action == QueueActions.download && RabbitHelper.TrySerialize(data, out ReportSource source) && source is not null)
                await reportLoader.LoadReportsAsync(source);

            return true;
        }
    }
}
