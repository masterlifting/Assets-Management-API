using CommonServices.RabbitServices;

using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.Services.ReportServices;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommonServices.Models.Dto.CompanyReports;
using static IM.Service.Company.Reports.Enums;
// ReSharper disable InvertIf

namespace IM.Service.Company.Reports.Services.RabbitServices.Implementations
{
    public class RabbitReportService : IRabbitActionService
    {
        private readonly ReportLoader reportLoader;
        public RabbitReportService(ReportLoader reportLoader) => this.reportLoader = reportLoader;

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
        {
            if (entity == QueueEntities.Report && action == QueueActions.GetData && RabbitHelper.TrySerialize(data, out Ticker? ticker))
                await reportLoader.LoadAsync(ticker!);

            return true;
        }
    }
}
