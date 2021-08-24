using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Companies.Reports.Api.DataAccess.DataEnums;
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

        public async Task<int> LoadReportsAsync()
        {
            int loadedReportsCount = 0;

            var activeSources = await context.ReportSources.Where(x => x.IsActive).Select(x => new ReportSource
            {
                Id = x.Id,
                ReportSourceTypeId = x.ReportSourceTypeId,
                IsActive = x.IsActive,
                Value = x.Value,
                TickerName = x.TickerName
            }).ToArrayAsync();
            var groupedSources = activeSources.GroupBy(x => x.ReportSourceTypeId);

            foreach (var group in groupedSources)
            {
                var sourceType = Enum.Parse<ReportSourceTypes>(group.Key.ToString());

                var lastReports = await GetLastReportsAsync(group.Select(x => x.Id));
                var reportsWithoutLastQuarter = GetReportsWithoutLastQuarter(lastReports).ToArray();
                var reportSources = reportsWithoutLastQuarter.Join(group, x => x.ReportSourceId, y => y.Id, (_, y) => y).ToArray();

                foreach (var item in reportSources.Join(lastReports, x => x.Id, y => y.ReportSourceId, (x, y) => new { source = x, lastReport = y }))
                {
                    await Task.Delay(5000);
                    var loadedReports = await parser.GetReportsAsync(sourceType, item.source);
                    (Report[] toAdd, Report[] toUpdate) = SeparateReports(loadedReports, item.lastReport);
                    int saveResult = await SaveReportsAsync(toAdd, toUpdate);
                    loadedReportsCount += saveResult;
                }
            }

            return loadedReportsCount;
        }
        public async Task<int> LoadReportsAsync(ReportSource source)
        {
            int result = 0;

            var _source = await context.ReportSources.FindAsync(source.Id);
            
            if (_source is not null)
            {
                var sourceType = Enum.Parse<ReportSourceTypes>(_source.ReportSourceTypeId.ToString());
                var lastReport = await GetLastReportAsync(_source);

                if (IsMissingLastQuarter(lastReport.Year, lastReport.Quarter))
                {
                    var loadedReports = await parser.GetReportsAsync(sourceType, _source);
                    (Report[] toAdd, Report[] toUpdate) = SeparateReports(loadedReports, lastReport);
                    result = await SaveReportsAsync(toAdd, toUpdate);
                }
            }

            Console.WriteLine($"saved reports count: {result}");

            return result;
        }

        private async Task<List<Report>> GetLastReportsAsync(IEnumerable<int> reportSourceIds)
        {
            var result = new List<Report>();
            if (reportSourceIds is null || !reportSourceIds.Any())
                return result;

            var sources = context.ReportSources.Where(x => reportSourceIds.Contains(x.Id));
            var reports = await context.Reports.Join(sources, x => x.ReportSourceId, y => y.Id, (x, _) => x).ToArrayAsync();
            var groupedReports = reports.GroupBy(x => x.ReportSourceId);

            var sourceIdsWithOutReports = reportSourceIds.Except(reports.Select(x => x.ReportSourceId));
            if (sourceIdsWithOutReports.Any())
                result.AddRange(sources.Where(x => sourceIdsWithOutReports.Contains(x.Id)).Select(x => new Report
                {
                    ReportSourceId = x.Id,
                    ReportSource = x,
                    Year = 0,
                    Quarter = 0
                }));

            foreach (var item in groupedReports)
                result.Add(FindLastReport(item));

            return result;
        }
        private async Task<Report> GetLastReportAsync(ReportSource source)
        {
            var reports = await context.Reports.Where(x => x.ReportSourceId == source.Id).ToArrayAsync();
            return reports.Any() ? FindLastReport(reports) : new() { ReportSourceId = source.Id, ReportSource = source, Year = 0, Quarter = 0 };
        }

        private async Task<int> SaveReportsAsync(Report[] toAdd, Report[] toUpdate)
        {
            if (toAdd is not null && toAdd.Any())
                await context.Reports.AddRangeAsync(toAdd);
            
            if (toUpdate is not null && toUpdate.Any())
                for (int j = 0; j < toUpdate.Length; j++)
                {
                    var report = await context.Reports.FindAsync(toUpdate[j].ReportSourceId, toUpdate[j].Year, toUpdate[j].Quarter);

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

            return await context.SaveChangesAsync();
        }
    }
}