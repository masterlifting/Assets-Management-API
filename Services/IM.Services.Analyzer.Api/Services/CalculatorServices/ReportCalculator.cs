﻿using CommonServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.Clients;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces;

using System;
using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class ReportCalculator : IAnalyzerCalculator<Report>
    {
        private readonly AnalyzerRepository<Report> repository;
        private readonly ReportsClient reportsClient;
        private readonly PricesClient pricesClient;

        public ReportCalculator(AnalyzerRepository<Report> repository, ReportsClient reportsClient, PricesClient pricesClient)
        {
            this.repository = repository;
            this.reportsClient = reportsClient;
            this.pricesClient = pricesClient;
        }

        public async Task<bool> IsSetCalculatingStatusAsync(Report[] reports)
        {
            for (int i = 0; i < reports.Length; i++)
                reports[i].StatusId = (byte)StatusType.Calculating;

            var updatedResult = await repository.UpdateAsync(reports, $"reports calculating status count: {reports.Length}");

            return updatedResult.Length == reports.Length;
        }
        public async Task CalculateAsync()
        {
            var reports = await repository.FindAsync(x =>
                    x.StatusId == ((byte)StatusType.ToCalculate)
                    || x.StatusId == ((byte)StatusType.CalculatedPartial)
                    || x.StatusId == ((byte)StatusType.Error));

            if (reports.Any() && await IsSetCalculatingStatusAsync(reports))
                foreach (var reportGroup in reports.GroupBy(x => x.TickerName))
                {
                    var firstElement = reportGroup.OrderBy(x => x.Year).ThenBy(x => x.Quarter).First();

                    var reportTargetDate = CommonHelper.SubstractQuarter(firstElement.Year, firstElement.Quarter);
                    var priceTargetDate = CommonHelper.GetQuarterFirstDate(reportTargetDate.year, reportTargetDate.quarter);

                    var response = await reportsClient.GetReportsAsync(reportGroup.Key, new(reportTargetDate.year, reportTargetDate.quarter), new(1, int.MaxValue));

                    if (!response.Errors.Any() && response.Data!.Count > 0)
                    {
                        var pricesResponse = await pricesClient.GetPricesAsync(reportGroup.Key, new(priceTargetDate.year, priceTargetDate.month), new(1, int.MaxValue));

                        Report[] result = reportGroup.ToArray();

                        try
                        {
                            var calculator = new ReportComporator(response.Data.Items, pricesResponse.Data?.Items);
                            result = calculator.GetComparedSample();
                        }
                        catch (Exception ex)
                        {
                            for (int k = 0; k < result.Length; k++)
                                result[k].StatusId = (byte)StatusType.Error;

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"calculating reports for {reportGroup.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        await repository.CreateOrUpdateAsync(result, new ReportComparer(), $"analyzer reports");
                    }
                }
        }
    }
}