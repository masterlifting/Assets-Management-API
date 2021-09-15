using CommonServices;
using CommonServices.Models.Entity;

using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using System;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices
{
    public class ReportCalculator : IAnalyzerCalculator<Report>
    {
        private readonly RepositorySet<Report> repository;
        private readonly CompanyReportsClient reportsClient;
        private readonly CompanyPricesClient pricesClient;

        public ReportCalculator(
            RepositorySet<Report> repository,
            CompanyReportsClient reportsClient,
            CompanyPricesClient pricesClient)
        {
            this.repository = repository;
            this.reportsClient = reportsClient;
            this.pricesClient = pricesClient;
        }

        public async Task<bool> IsSetCalculatingStatusAsync(Report[] reports)
        {
            foreach (var t in reports)
                t.StatusId = (byte)StatusType.Calculating;

            var (errors, _) = await repository.UpdateAsync(reports, $"reports set calculating status count: {reports.Length}");

            return !errors.Any();
        }
        public async Task<bool> CalculateAsync()
        {
            var reports = await repository.FindAsync(x =>
                    x.StatusId == (byte)StatusType.ToCalculate
                    || x.StatusId == (byte)StatusType.CalculatedPartial
                    || x.StatusId == (byte)StatusType.Error);

            if (!reports.Any())
                return false;

            if (await IsSetCalculatingStatusAsync(reports))
                foreach (var reportGroup in reports.GroupBy(x => x.TickerName))
                {
                    var firstElement = reportGroup.OrderBy(x => x.Year).ThenBy(x => x.Quarter).First();
                    Report[] result = reportGroup.ToArray();

                    var reportTargetDate = CommonHelper.SubstractQuarter(firstElement.Year, firstElement.Quarter);
                    var priceTargetDate = CommonHelper.GetQuarterFirstDate(reportTargetDate.year, reportTargetDate.quarter);

                    try
                    {
                        var response = await reportsClient.GetReportsAsync(reportGroup.Key, new(reportTargetDate.year, reportTargetDate.quarter), new(1, int.MaxValue));

                        if (response.Errors.Any() || response.Data!.Count <= 0)
                            continue;

                        var pricesResponse = await pricesClient.GetPricesAsync(reportGroup.Key, new(priceTargetDate.year, priceTargetDate.month, 1), new(1, int.MaxValue));


                        var calculator = new ReportComporator(response.Data.Items, pricesResponse.Data?.Items);
                        result = calculator.GetComparedSample();
                    }
                    catch (Exception ex)
                    {
                        foreach (var report in result)
                            report.StatusId = (byte)StatusType.Error;

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"calculating reports for {reportGroup.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    await repository.CreateUpdateAsync(result, new ReportComparer(), "analyzer reports");
                }

            return true;
        }
    }
}
