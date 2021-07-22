using IM.Services.Companies.Reports.Api.Clients;
using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.Services.Background.ReportUpdaterBackgroundServices.Interfaces;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Companies.Reports.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Reports.Api.Services.Background.ReportUpdaterBackgroundServices.Implementations
{
    public class ReportUpdater : IReportUpdater
    {
        private readonly ReportsContext context;
        private readonly Dictionary<ReportSourceTypes, IClientReportUpdater> updaters;
        public ReportUpdater(ReportsContext context, InvestingClient client)
        {
            this.context = context;
            updaters = new()
            {
                { ReportSourceTypes.investing, new InvestingUpdater(client) }
            };
        }
        public async Task<int> UpdateReportsAsync()
        {
            int updatedReportsCount = 0;

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
                    var loadedReports = await updaters[sourceType].GetReportsAsync(item.source);
                    (Report[] toAdd, Report[] toUpdate) = SeparateReports(loadedReports, item.lastReport);
                    int saveResult = await SaveReportsAsync(toAdd, toUpdate);
                    updatedReportsCount += saveResult;
                }
            }

            return updatedReportsCount;
        }
        public static byte GetQuarter(int month) => month switch
        {
            int x when x >= 1 && x < 4 => 1,
            int x when x >= 4 && x < 7 => 2,
            int x when x >= 7 && x < 10 => 3,
            int x when x >= 10 && x <= 12 => 4,
            _ => throw new NotSupportedException()
        };


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
                result.Add(GetLast(item));

            return result;

            static Report GetLast(IEnumerable<Report> items) => items.GroupBy(x => x.Year).OrderBy(x => x.Key).Last().OrderBy(x => x.Quarter).Last();
        }
        private static IEnumerable<Report> GetReportsWithoutLastQuarter(IEnumerable<Report> lastReports) => lastReports.Where(x => IsMissingLastQuarter(x.Year, x.Quarter));

        private static (Report[] toAdd, Report[] toUpdate) SeparateReports(Report[] loadedReports, Report lastReport)
        {
            var reportsToAdd = Array.Empty<Report>();
            var reportsToUpdate = Array.Empty<Report>();

            if (loadedReports.Any())
            {
                reportsToAdd = loadedReports.Where(x => IsNewQuarter((x.Year, x.Quarter), (lastReport.Year, lastReport.Quarter))).ToArray();
                reportsToUpdate = loadedReports.Where(x => !IsNewQuarter((x.Year, x.Quarter), (lastReport.Year, lastReport.Quarter))).ToArray();
            }

            return (reportsToAdd, reportsToUpdate);
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

        private static bool IsMissingLastQuarter(int lastYear, byte lastQuarter)
        {
            int controlYear = DateTime.UtcNow.Year;
            byte controlQuarter = GetQuarter(DateTime.UtcNow.Month);

            if (controlQuarter == 1)
            {
                controlYear--;
                controlQuarter = 4;
            }
            else
                controlQuarter--;

            return IsNewQuarter((controlYear, controlQuarter), (lastYear, lastQuarter));
        }
        private static bool IsNewQuarter((int year, byte quarter) current, (int year, byte qarter) last) =>
            current.year > last.year
            || (current.year == last.year && current.quarter > last.qarter);
    }
}