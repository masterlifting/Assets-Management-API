using CommonServices;
using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Entity;

using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.AspNetCore.Http;

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;
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

        public async Task<bool> IsSetCalculatingStatusAsync(Report[] reports, string info)
        {
            foreach (var report in reports)
                report.StatusId = (byte)StatusType.Calculating;

            var (errors, _) = await repository.UpdateAsync(reports, $"report calculating status for '{info}'");

            return !errors.Any();
        }

        public async Task<bool> CalculateAsync()
        {
            var reports = await repository.GetSampleAsync(x =>
                    x.StatusId == (byte)StatusType.ToCalculate
                    || x.StatusId == (byte)StatusType.CalculatedPartial
                    || x.StatusId == (byte)StatusType.Error);

            if (!reports.Any())
                return false;

            foreach (var group in reports.GroupBy(x => x.TickerName))
            {
                var firstElement = group.OrderBy(x => x.Year).ThenBy(x => x.Quarter).First();

                var (targetYear, targetQuarter) = CommonHelper.SubtractQuarter(firstElement.Year, firstElement.Quarter);
                var (year, month, day) = CommonHelper.GetQuarterFirstDate(targetYear, targetQuarter);

                await BaseCalculateAsync(group, new DateTime(year, month, day), targetYear, targetQuarter);
            }

            return true;
        }
        public async Task<bool> CalculateAsync(DateTime dateStart)
        {
            var reports = await repository.GetSampleAsync(x => x.Year >= dateStart.Year);

            if (!reports.Any())
                return false;

            foreach (var group in reports.GroupBy(x => x.TickerName))
            {
                var (targetYear, targetQuarter) = CommonHelper.SubtractQuarter(dateStart);
                var (year, month, day) = CommonHelper.GetQuarterFirstDate(targetYear, targetQuarter);

                await BaseCalculateAsync(group, new DateTime(year, month, day), targetYear, targetQuarter);
            }

            return true;
        }

        private async Task BaseCalculateAsync(IGrouping<string, Report> group, DateTime dateStart, int year, byte quarter)
        {
            var reportGroup = group.ToArray();

            if (!await IsSetCalculatingStatusAsync(reportGroup, group.Key))
                throw new DataException($"set report calculating status for '{group.Key}' failed");

            try
            {
                var (dtoReports, dtoPrices) = await GetCalculatingDataAsync(group.Key, dateStart, year, quarter);

                if (!dtoReports.Any())
                    throw new DataException($"reports for '{group.Key}' not found!");

                var calculator = new ReportComparator(dtoReports, dtoPrices);
                await repository.CreateUpdateAsync(
                    calculator.GetComparedSample(),
                    new ReportComparer(),
                    $"report calculated result for {group.Key}");
            }
            catch (Exception ex)
            {
                foreach (var price in group)
                    price.StatusId = (byte)StatusType.Error;

                await repository.CreateUpdateAsync(reportGroup, new ReportComparer(), $"calculate reports for {group.Key}");

                throw new ArithmeticException($"report calculated for '{group.Key}' failed! \nError: {ex.Message}");
            }
        }
        private async Task<(ReportGetDto[] dtoReports, PriceGetDto[]? dtoPrices)> GetCalculatingDataAsync(string ticker, DateTime date, int year, byte quarter)
        {
            var reportResponse = await reportsClient.Get<ReportGetDto>(
                "reports",
                GetQueryString(HttpRequestFilterType.More, ticker, year, quarter),
                new(1, int.MaxValue));

            if (reportResponse.Errors.Any())
                throw new BadHttpRequestException(string.Join('\n', reportResponse.Errors));

            var pricesResponse = await pricesClient.Get<PriceGetDto>(
                "prices",
                GetQueryString(HttpRequestFilterType.More, ticker, date.Year, date.Month, date.Day),
                new(1, int.MaxValue));

            return pricesResponse.Errors.Any()
                ? throw new BadHttpRequestException(string.Join('\n', reportResponse.Errors))
                : (reportResponse.Data!.Items, pricesResponse.Data?.Items);
        }
    }
}
