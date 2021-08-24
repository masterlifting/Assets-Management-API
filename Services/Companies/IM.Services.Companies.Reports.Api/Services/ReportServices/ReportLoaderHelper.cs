using IM.Services.Companies.Reports.Api.DataAccess.Entities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Companies.Reports.Api.Services.ReportServices
{
    public static class ReportLoaderHelper
    {
        public static byte GetQuarter(int month) => month switch
        {
            int x when x >= 1 && x < 4 => 1,
            int x when x >= 4 && x < 7 => 2,
            int x when x >= 7 && x < 10 => 3,
            int x when x >= 10 && x <= 12 => 4,
            _ => throw new NotSupportedException()
        };
        public static IEnumerable<Report> GetReportsWithoutLastQuarter(IEnumerable<Report> lastReports) => lastReports.Where(x => IsMissingLastQuarter(x.Year, x.Quarter));
        public static Report FindLastReport(IEnumerable<Report> reports) => reports.GroupBy(x => x.Year).OrderBy(x => x.Key).Last().OrderBy(x => x.Quarter).Last();
        public static (Report[] toAdd, Report[] toUpdate) SeparateReports(Report[] loadedReports, Report lastReport)
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
        public static bool IsMissingLastQuarter(int lastYear, byte lastQuarter)
        {
            int controlYear = DateTime.UtcNow.Year;
            byte controlQuarter = ReportLoaderHelper.GetQuarter(DateTime.UtcNow.Month);

            if (controlQuarter == 1)
            {
                controlYear--;
                controlQuarter = 4;
            }
            else
                controlQuarter--;

            return IsNewQuarter((controlYear, controlQuarter), (lastYear, lastQuarter));
        }
        public static bool IsNewQuarter((int year, byte quarter) current, (int year, byte qarter) last) =>
            current.year > last.year
            || (current.year == last.year && current.quarter > last.qarter);
    }
}
