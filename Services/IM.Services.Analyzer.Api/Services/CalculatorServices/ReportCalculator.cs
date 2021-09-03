using CommonServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.Clients;
using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class ReportCalculator : IAnalyzerCalculator<Report>
    {
        private readonly AnalyzerContext context;
        private readonly EntityRepository<Report, AnalyzerContext> repository;
        private readonly ReportsClient reportsClient;
        private readonly PricesClient pricesClient;

        public ReportCalculator(AnalyzerContext context, EntityRepository<Report, AnalyzerContext> repository, ReportsClient reportsClient, PricesClient pricesClient)
        {
            this.context = context;
            this.repository = repository;
            this.reportsClient = reportsClient;
            this.pricesClient = pricesClient;
        }

        public async Task<Report[]> GetDataAsync() => await context.Reports.Where(x =>
                    x.StatusId == ((byte)StatusType.ToCalculate)
                    || x.StatusId == ((byte)StatusType.CalculatedPartial)
                    || x.StatusId == ((byte)StatusType.Error))
                    .ToArrayAsync();
        public async Task<bool> IsSetCalculatingStatusAsync(Report[] reports)
        {
            for (int i = 0; i < reports.Length; i++)
                reports[i].StatusId = (byte)StatusType.Calculating;

            return await context.SaveChangesAsync() == reports.Length;
        }
        public async Task CalculateAsync()
        {
            var reports = await GetDataAsync();
            if (reports.Any() && await IsSetCalculatingStatusAsync(reports))
                foreach (var reportGroup in reports.GroupBy(x => x.TickerName))
                {
                    var firstReport = reportGroup.First();

                    var reportTargetDate = CommonHelper.SubstractQuarter(firstReport.Year, firstReport.Quarter);
                    var priceTargetDate = CommonHelper.GetQuarterFirstDate(reportTargetDate.year, reportTargetDate.quarter);

                    var reportResponse = await reportsClient.GetReportsAsync(reportGroup.Key, new(reportTargetDate.year, reportTargetDate.quarter), new(1, int.MaxValue));

                    if (!reportResponse.Errors.Any() && reportResponse.Data!.Count > 0)
                    {
                        var pricesResponse = await pricesClient.GetPricesAsync(reportGroup.Key, new(priceTargetDate.year, priceTargetDate.month), new(1, int.MaxValue));

                        Report[] result = reportGroup.ToArray();
                        try
                        {
                            var calculator = new ReportComporator(reportResponse.Data.Items, pricesResponse.Data?.Items);
                            result = calculator.GetComparedSample();
                        }
                        catch (Exception ex)
                        {
                            for (int k = 0; k < result.Length; k++)
                                result[k].StatusId = (byte)StatusType.Error;

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Calculating reports for {reportGroup.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        for (int k = 0; k < result.Length; k++)
                            await repository.CreateOrUpdateAsync(result[k], $"analyzer report for {result[k].TickerName}");
                    }
                }
        }
    }
}
