using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.RabbitServices;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.Services.ReportServices;
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
                var reports = await reportLoader.LoadReportsAsync(ticker!);
                if (reports.Length > 0)
                {
                    var sourceType = Enum.Parse<ReportSourceTypes>(ticker!.SourceTypeId.ToString(), true).ToString();
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

                    foreach (var report in reports)
                        publisher.PublishTask(
                            QueueNames.CompanyAnalyzer
                            , QueueEntities.Report
                            , QueueActions.GetLogic
                            , JsonSerializer.Serialize(new AnalyzerReportDto
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
