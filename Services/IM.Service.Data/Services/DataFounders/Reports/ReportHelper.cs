using IM.Service.Common.Net;

namespace IM.Service.Data.Services.DataFounders.Reports;

public static class ReportHelper
{
    public static bool IsMissingLastQuarter(int year, byte quarter)
    {
        var (controlYear, controlQuarter) = Helper.QuarterHelper.SubtractQuarter(DateOnly.FromDateTime(DateTime.UtcNow));
        return controlYear > year || controlYear == year && controlQuarter > quarter;
    }
}