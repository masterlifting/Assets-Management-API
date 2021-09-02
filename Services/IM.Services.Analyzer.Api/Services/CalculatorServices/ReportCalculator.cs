using CommonServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.Clients;
using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class ReportCalculator : IAnalyzerCalculator
    {
        private readonly IServiceProvider services;
        private readonly AnalyzerContext context;
        private readonly ReportsClient reportsClient;
        private readonly PricesClient pricesClient;

        public ReportCalculator(IServiceProvider services, AnalyzerContext context, ReportsClient reportsClient, PricesClient pricesClient)
        {
            this.services = services;
            this.context = context;
            this.reportsClient = reportsClient;
            this.pricesClient = pricesClient;
        }

        async Task<Report[]> GetReportsAsync() => await context.Reports.Where(x =>
                    x.StatusId == ((byte)StatusType.ToCalculate)
                    && x.StatusId == ((byte)StatusType.CalculatedPartial)
                    && x.StatusId == ((byte)StatusType.Error))
                    .ToArrayAsync();
        async Task<bool> IsSetCalculatingStatusAsync(Report[] reports)
        {
            for (int i = 0; i < reports.Length; i++)
                reports[i].StatusId = (byte)StatusType.Calculating;

            return await context.SaveChangesAsync() == reports.Length;
        }
        public async Task CalculateAsync()
        {
            var reports = await GetReportsAsync();
            if (reports.Any() && await IsSetCalculatingStatusAsync(reports))
            {
                var repository = services.CreateScope().ServiceProvider.GetRequiredService<EntityRepository<Report, AnalyzerContext>>();

                var tickerReports = reports.GroupBy(x => x.TickerName);

                foreach (var i in tickerReports)
                    foreach (var j in i.GroupBy(x => x.ReportSourceId))
                    {
                        var targetReports = j.OrderBy(x => x.Year).ThenBy(x => x.Quarter);
                        var firstReport = targetReports.First();

                        var reportTargetDate = CommonHelper.SubstractQuarter(firstReport.Year, firstReport.Quarter);
                        var priceTargetDate = CommonHelper.GetQuarterFirstDate(reportTargetDate.year, reportTargetDate.quarter);

                        var reportResponse = await reportsClient.GetReportsAsync(i.Key, j.Key, new(reportTargetDate.year, reportTargetDate.quarter), new(1, int.MaxValue));

                        if (!reportResponse.Errors.Any() && reportResponse.Data!.Count > 0)
                        {
                            var pricesResponse = await pricesClient.GetPricesAsync(i.Key, new(priceTargetDate.year, priceTargetDate.month), new(1, int.MaxValue));

                            Report[] result = targetReports.ToArray();
                            try
                            {
                                var calculator = new ReportComporator(reportResponse.Data.Items, pricesResponse.Data?.Items);
                                result = calculator.GetCoparedSample();
                            }
                            catch (Exception ex)
                            {
                                for (int k = 0; k < result.Length; k++)
                                    result[k].StatusId = (byte)StatusType.Error;

                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Calculating reports for {i.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }

                            for (int k = 0; k < result.Length; k++)
                                await repository.CreateOrUpdateAsync(new { result[k].ReportSourceId, result[k].Year, result[k].Quarter }, result[k], $"analyzer report for {result[k].TickerName}");
                        }
                    }
            }
        }
    }
}
