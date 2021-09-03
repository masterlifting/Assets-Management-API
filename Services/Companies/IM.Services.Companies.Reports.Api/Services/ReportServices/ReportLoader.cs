using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Companies.Reports.Api.Services.ReportServices.ReportLoaderHelper;

namespace IM.Services.Companies.Reports.Api.Services.ReportServices
{
    public class ReportLoader
    {
        private readonly ReportsContext context;
        private readonly ReportParser parser;
        public ReportLoader(ReportsContext context, ReportParser parser)
        {
            this.context = context;
            this.parser = parser;
        }

        public async Task<Report[]> LoadReportsAsync(Ticker ticker)
        {
            var result = Array.Empty<Report>();

            var _ticker = await context.Tickers.FindAsync(ticker.Name);

            if (_ticker is not null)
            {
                var lastReport = await GetLastReportAsync(_ticker);

                if (IsMissingLastQuarter(lastReport.Year, lastReport.Quarter))
                {
                    var loadedReports = await parser.GetReportsAsync(_ticker);
                    (Report[] toAdd, Report[] toUpdate) = SeparateReports(loadedReports, lastReport);
                    result = await SaveReportsAsync(toAdd, toUpdate);
                }
            }

            Console.WriteLine($"\nSaved report count: {result.Length}");

            return result;
        }
        public async Task<Report[]> LoadReportsAsync()
        {
            var tickers = await context.Tickers.ToArrayAsync();
            var result = new List<Report>(tickers.Length);

            foreach (var ticker in tickers)
            {
                var lastReport = await GetLastReportAsync(ticker);

                if (IsMissingLastQuarter(lastReport.Year, lastReport.Quarter))
                {
                    var loadedReports = await parser.GetReportsAsync(ticker);
                    (Report[] toAdd, Report[] toUpdate) = SeparateReports(loadedReports, lastReport);
                    var reports = await SaveReportsAsync(toAdd, toUpdate);
                    result.AddRange(reports);
                }
            }

            return result.ToArray();
        }

        private async Task<Report> GetLastReportAsync(Ticker ticker)
        {
            var reports = await context.Reports.Where(x => x.TickerName == ticker.Name).ToArrayAsync();
            return reports.Any() ? FindLastReport(reports) : new() { TickerName = ticker.Name, Year = 0, Quarter = 0 };
        }
        private async Task<Report[]> SaveReportsAsync(Report[] toAdd, Report[] toUpdate)
        {
            var result = Array.Empty<Report>();

            if (toAdd is not null && toAdd.Any())
            {
                await context.Reports.AddRangeAsync(toAdd);
                result = result.Concat(toAdd).ToArray();
            }

            if (toUpdate is not null && toUpdate.Any())
            {
                for (int j = 0; j < toUpdate.Length; j++)
                {
                    var report = await context.Reports.FindAsync(toUpdate[j].TickerName, toUpdate[j].Year, toUpdate[j].Quarter);

                    report.Turnover = toUpdate[j].Turnover;
                    report.LongTermDebt = toUpdate[j].LongTermDebt;
                    report.Asset = toUpdate[j].Asset;
                    report.CashFlow = toUpdate[j].CashFlow;
                    report.Obligation = toUpdate[j].Obligation;
                    report.ProfitGross = toUpdate[j].ProfitGross;
                    report.ProfitNet = toUpdate[j].ProfitNet;
                    report.Revenue = toUpdate[j].Revenue;
                    report.ShareCapital = toUpdate[j].ShareCapital;
                    report.Dividend = toUpdate[j].Dividend;
                }

                result = result.Concat(toUpdate).ToArray();
            }

            await context.SaveChangesAsync();

            return result;
        }
    }
}