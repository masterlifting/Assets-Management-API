using CommonServices;
using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
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
            foreach (var report in reports)
                report.StatusId = (byte)StatusType.Calculating;

            var (errors, _) = await repository.UpdateAsync(reports, $"reports calculating status count: {reports.Length}");

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

            foreach (var reportGroup in reports.GroupBy(x => x.TickerName))
            {
                Report[] result = reportGroup.ToArray();

                if (!await IsSetCalculatingStatusAsync(result))
                    continue;

                var firstElement = reportGroup.OrderBy(x => x.Year).ThenBy(x => x.Quarter).First();

                var (reportYear, reportQuarter) = CommonHelper.SubtractQuarter(firstElement.Year, firstElement.Quarter);
                var (priceYear, priceMonth, priceDay) = CommonHelper.GetQuarterFirstDate(reportYear, reportQuarter);

                try
                {
                    var (dtoReports, dtoPrices) = await GeReportsAsync(reportGroup.Key, new DateTime(priceYear, priceMonth, priceDay), reportYear, reportQuarter);

                    var calculator = new ReportComparator(dtoReports, dtoPrices);
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

                await repository.CreateUpdateAsync(result, new ReportComparer(), "report calculator");
            }

            return true;
        }
        public async Task<bool> CalculateAsync(DateTime dateStart)
        {
            var reports = await repository.FindAsync(x => x.StatusId != (byte)StatusType.Calculating);

            if (!reports.Any())
                return false;

            foreach (var reportGroup in reports.GroupBy(x => x.TickerName))
            {
                Report[] result = reportGroup.ToArray();

                if (!await IsSetCalculatingStatusAsync(result))
                    continue;

                var (targetYear, targetQuarter) = CommonHelper.SubtractQuarter(dateStart);
                var (year, month, day) = CommonHelper.GetQuarterFirstDate(targetYear, targetQuarter);


                try
                {
                    var (dtoReports, dtoPrices) = await GeReportsAsync(reportGroup.Key, new DateTime(year, month, day), targetYear, targetQuarter);

                    var calculator = new ReportComparator(dtoReports, dtoPrices);
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

                await repository.CreateUpdateAsync(result, new ReportComparer(), "report calculator");
            }

            return true;
        }
        private async Task<(ReportDto[] dtoReports, PriceDto[]? dtoPrices)> GeReportsAsync(string ticker, DateTime date, int year, byte quarter)
        {
            ResponseModel<PaginationResponseModel<ReportDto>>? reportResponse = null;
            ResponseModel<PaginationResponseModel<PriceDto>>? pricesResponse = null;

            try
            {
                reportResponse = await reportsClient.GetAsync(ticker, new(year, quarter), new(1, int.MaxValue));
                pricesResponse = await pricesClient.GetAsync(ticker, new(date.Year, date.Month, date.Day), new(1, int.MaxValue));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"requests for reports calculating for '{ticker}' failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            return reportResponse!.Errors.Any()
                ? (Array.Empty<ReportDto>(), null)
                : (reportResponse.Data!.Items, pricesResponse!.Data?.Items);
        }
    }
}
