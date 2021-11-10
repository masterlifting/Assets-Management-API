using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.AspNetCore.Http;

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Http.Companies;
using IM.Service.Common.Net.RepositoryService.Comparators;
using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.CommonHelper;
using static IM.Service.Common.Net.HttpServices.QueryStringBuilder;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices
{
    public class ReportCalculator : IAnalyzerCalculator<Report>
    {
        private readonly RepositorySet<Report> repository;
        private readonly CompanyDataClient dataClient;

        public ReportCalculator(
            RepositorySet<Report> repository,
            CompanyDataClient dataClient)
        {
            this.repository = repository;
            this.dataClient = dataClient;
        }

        public async Task<bool> IsSetCalculatingStatusAsync(Report[] reports, string info)
        {
            foreach (var report in reports)
                report.StatusId = (byte)StatusType.Calculating;

            return (await repository.UpdateAsync(reports, $"report calculating status for '{info}'")).error is not null;
        }
        public async Task<bool> CalculateAsync()
        {
            var reports = await repository.GetSampleAsync(x =>
                x.StatusId == (byte)StatusType.ToCalculate
                || x.StatusId == (byte)StatusType.CalculatedPartial
                || x.StatusId == (byte)StatusType.Error);

            if (!reports.Any())
                return false;

            foreach (var group in reports.GroupBy(x => x.CompanyId))
                await BaseCalculateAsync(group);

            return true;
        }

        private async Task BaseCalculateAsync(IGrouping<string, Report> group)
        {
            var companyId = group.Key;
            
            var orderedReports = group.OrderBy(x => x.Year).ThenBy(x => x.Quarter).ToArray();
            var (targetYear, targetQuarter) = SubtractQuarter(orderedReports[0].Year, orderedReports[0].Quarter);
            (targetYear, targetQuarter) = SubtractQuarter(targetYear, targetQuarter);

            if (!await IsSetCalculatingStatusAsync(orderedReports, companyId))
                throw new DataException($"set report calculating status for '{companyId}' failed");

            try
            {
                var calculateResult = await GetCalculatedResultAsync(companyId, targetYear, targetQuarter);
                await repository.CreateUpdateAsync(calculateResult, new CompanyQuarterComparer<Report>(), $"report calculated result for {companyId}");
            }
            catch (Exception exception)
            {
                foreach (var report in orderedReports)
                    report.StatusId = (byte)StatusType.Error;

                await repository.CreateUpdateAsync(orderedReports, new CompanyQuarterComparer<Report>(), $"calculate reports for {companyId}");

                throw new ArithmeticException($"report calculated for '{companyId}' failed! \nError: {exception.Message}");
            }
        }
        private async Task<(ReportGetDto[] dtoReports, PriceGetDto[]? dtoPrices)> GetCalculatingDataAsync(string companyId, int year, byte quarter)
        {
            var reportResponse = await dataClient.Get<ReportGetDto>(
                "reports",
                GetQueryString(HttpRequestFilterType.More, companyId, year, quarter),
                new(1, int.MaxValue));

            if (reportResponse.Errors.Any())
                throw new BadHttpRequestException(string.Join('\n', reportResponse.Errors));

            var priceDate = GetQuarterFirstDate(year, quarter);

            var pricesResponse = await dataClient.Get<PriceGetDto>(
                "prices",
                GetQueryString(HttpRequestFilterType.More, companyId, priceDate.year, priceDate.month, priceDate.day),
                new(1, int.MaxValue));

            return pricesResponse.Errors.Any()
                ? throw new BadHttpRequestException(string.Join('\n', reportResponse.Errors))
                : (reportResponse.Data!.Items, pricesResponse.Data?.Items);
        }
        private async Task<Report[]> GetCalculatedResultAsync(string companyId, int targetYear, byte targetQuarter)
        {
            var (dtoReports, dtoPrices) = await GetCalculatingDataAsync(companyId, targetYear, targetQuarter);

            if (!dtoReports.Any())
                throw new DataException($"reports for '{companyId}' not found!");

            var calculator = new ReportComparator(dtoReports, dtoPrices);
            return calculator.GetComparedSample();
        }
    }
}
