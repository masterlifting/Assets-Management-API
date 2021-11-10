using IM.Service.Common.Net;

using IM.Service.Company.Data.Models.Data;

using System;

namespace IM.Service.Company.Data.Services.DataServices.Reports
{
    public static class ReportHelper
    {
        public static bool IsMissingLastQuarter(ReportDataConfigModel lastReport)
        {
            var (controlYear, controlQuarter) = CommonHelper.SubtractQuarter(DateTime.UtcNow);

            var isNew = controlYear > lastReport.Year || controlYear == lastReport.Year && controlQuarter > lastReport.Quarter;

            if (isNew)
                return isNew;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Last quarter is actual for '{lastReport.CompanyId}'. Reports will not be loaded.");
            Console.ForegroundColor = ConsoleColor.Gray;

            return isNew;
        }

    }
}
