using System;
using IM.Service.Common.Net;

namespace IM.Service.Company.Data.Services.DataServices.Reports;

public static class ReportHelper
{
    public static bool IsMissingLastQuarter(int year, byte quarter)
    {
        var (controlYear, controlQuarter) = CommonHelper.QuarterHelper.SubtractQuarter(DateTime.UtcNow);
        return controlYear > year || controlYear == year && controlQuarter > quarter;
    }
}