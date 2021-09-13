﻿using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.RabbitServices;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Services.Company.Reports.DataAccess.Entities;
using IM.Services.Company.Reports.Services.ReportServices;
using static IM.Services.Company.Reports.Enums;

namespace IM.Services.Company.Reports.Services.RabbitServices.Implementations
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
                    var sourceType = Enum.Parse<Enums.ReportSourceTypes>(ticker.SourceTypeId.ToString(), true).ToString();
                    var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Calculator);

                    foreach (var report in reports)
                        publisher.PublishTask(
                            QueueNames.CompaniesAnalyzer
                            , QueueEntities.Report
                            , QueueActions.Calculate
                            , JsonSerializer.Serialize(new AnalyzerReportDto
                            {
                                TickerName = ticker.Name,
                                Year = report.Year,
                                Quarter = report.Quarter,
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