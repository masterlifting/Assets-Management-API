
using CommonServices.Models.AnalyzerService;
using CommonServices.RabbitServices;

using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.Services.ReportServices;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Text.Json;
using System.Threading.Tasks;

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

        public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data, IServiceScope scope)
        {
            if (entity == QueueEntities.report && action == QueueActions.download && RabbitHelper.TrySerialize(data, out ReportSource source) && source is not null)
            {
                var reports = await reportLoader.LoadReportsAsync(source);
                if (reports.Length > 0)
                {
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.calculator);
                    for (int i = 0; i < reports.Length; i++)
                        publisher.PublishTask(
                            QueueNames.companiesanalyzer
                            , QueueEntities.report
                            , QueueActions.calculate
                            , JsonSerializer.Serialize(new AnalyzerReportDto
                            {
                                TickerName = source.TickerName,
                                ReportSourceId = reports[i].ReportSourceId,
                                Year = reports[i].Year,
                                Quarter = reports[i].Quarter
                            }));
                }
            }
            else
                Console.WriteLine(nameof(RabbitReportService) + " error!");

            return true;
        }
    }
}
