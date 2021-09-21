using CommonServices.RabbitServices;

using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.Services.ReportServices;

using System;
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
        private readonly string rabbitConnectionString;

        public RabbitReportService(ReportLoader reportLoader, string rabbitConnectionString)
        {
            this.reportLoader = reportLoader;
            this.rabbitConnectionString = rabbitConnectionString;
        }

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
        {
            if (entity == QueueEntities.Report
                && action == QueueActions.GetData
                && RabbitHelper.TrySerialize(data, out Ticker? ticker))
            {
                var reports = await reportLoader.LoadAsync(ticker!);
                if (reports.Length > 0)
                {
                    var sourceType = Enum.Parse<ReportSourceTypes>(ticker!.SourceTypeId.ToString(), true).ToString();
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

                    foreach (var report in reports)
                        publisher.PublishTask(
                            QueueNames.CompanyAnalyzer
                            , QueueEntities.Report
                            , QueueActions.GetLogic
                            , JsonSerializer.Serialize(new ReportGetDto
                            {
                                TickerName = ticker.Name,
                                Year = report.Year,
                                Quarter = report.Quarter,
                                SourceType = sourceType
                            }));
                }
            }

            return true;
        }
    }
}
