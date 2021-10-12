using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Entity;

using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static CommonServices.CommonHelper;
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
                await BaseCalculateAsync(group);

            return true;
        }
        public async Task<bool> CalculateAsync(DateTime dateStart)
        {
            var quarter = GetQuarter(dateStart.Month);

            var reports = await repository.GetSampleAsync(x =>
               x.Year > dateStart.Year || x.Year == dateStart.Year && x.Quarter >= quarter);
            var tickers = await repository.GetDbSetBy<Ticker>().Select(x => x.Name).ToArrayAsync();

            if (!reports.Any() && !tickers.Any())
                return false;

            var groupedReports = reports.GroupBy(x => x.TickerName).ToArray();
            var tickersWithoutReports = tickers.Except(groupedReports.Select(x => x.Key)).ToArray();

            foreach (var ticker in tickersWithoutReports)
                await BaseCalculateAsync(ticker, dateStart);

            foreach (var group in groupedReports)
                await BaseCalculateAsync(group);

            return true;
        }

        private async Task BaseCalculateAsync(string ticker, DateTime dateStart)
        {
            try
            {
                var calculateResult = await GetCalculatedResultAsync(ticker, dateStart, dateStart.Year, GetQuarter(dateStart.Month));
                await repository.CreateAsync(calculateResult, new ReportComparer(), $"report calculated result for {ticker}");
            }
            catch (Exception ex)
            {
                throw new ArithmeticException($"report calculated for '{ticker}' failed! \nError: {ex.Message}");
            }
        }
        private async Task BaseCalculateAsync(IGrouping<string, Report> group)
        {
            var orderedReports = group.OrderBy(x => x.Year).ThenBy(x => x.Quarter).ToArray();

            var (targetYear, targetQuarter) = SubtractQuarter(orderedReports[0].Year, orderedReports[0].Quarter);
            (targetYear, targetQuarter) = SubtractQuarter(targetYear, targetQuarter);

            var (year, month, day) = GetQuarterFirstDate(targetYear, targetQuarter);
            var ticker = group.Key;

            if (!await IsSetCalculatingStatusAsync(orderedReports, ticker))
                throw new DataException($"set report calculating status for '{ticker}' failed");

            try
            {
                var calculateResult = await GetCalculatedResultAsync(ticker, new DateTime(year, month, day), targetYear, targetQuarter);
                await repository.CreateUpdateAsync(calculateResult, new ReportComparer(), $"report calculated result for {ticker}");
            }
            catch (Exception ex)
            {
                foreach (var report in orderedReports)
                    report.StatusId = (byte)StatusType.Error;

                await repository.CreateUpdateAsync(orderedReports, new ReportComparer(), $"calculate reports for {ticker}");

                throw new ArithmeticException($"report calculated for '{ticker}' failed! \nError: {ex.Message}");
            }
        }
        private async Task<Report[]> GetCalculatedResultAsync(string ticker, DateTime dateStart, int targetYear, byte targetQuarter)
        {
            var (dtoReports, dtoPrices) = await GetCalculatingDataAsync(ticker, dateStart, targetYear, targetQuarter);

            if (!dtoReports.Any())
                throw new DataException($"reports for '{ticker}' not found!");

            var calculator = new ReportComparator(dtoReports, dtoPrices);
            return calculator.GetComparedSample();
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
