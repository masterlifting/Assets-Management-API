using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;

using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.Services.ReportServices;

using System;
using System.Text.Json;
using System.Threading.Tasks;

using static IM.Services.Companies.Reports.Api.Enums;

namespace IM.Services.Companies.Reports.Api.Services.RabbitServices.Implementations
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
            if (entity == QueueEntities.Report && action == QueueActions.Download && RabbitHelper.TrySerialize(data, out Ticker ticker) && ticker is not null)
            {
                var reports = await reportLoader.LoadReportsAsync(ticker);
                if (reports.Length > 0)
                {
                    string sourceType = Enum.Parse<ReportSourceTypes>(ticker.SourceTypeId.ToString()).ToString();
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Calculator);

                    for (int i = 0; i < reports.Length; i++)
                        publisher.PublishTask(
                            QueueNames.CompaniesAnalyzer
                            , QueueEntities.Report
                            , QueueActions.Calculate
                            , JsonSerializer.Serialize(new AnalyzerReportDto
                            {
                                TickerName = ticker.Name,
                                Year = reports[i].Year,
                                Quarter = reports[i].Quarter,
                                SourceType = sourceType
                            }));
                }
            }
            else
                Console.WriteLine(nameof(RabbitReportService) + " error!");

            return true;
        }
    }
}
