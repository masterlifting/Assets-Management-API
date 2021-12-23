using IM.Service.Common.Net;

using IM.Service.Company.Data.Models.Data;

using System;
using Microsoft.Extensions.Logging;

namespace IM.Service.Company.Data.Services.DataServices.Reports
{
    public static class ReportHelper
    {
        public static bool IsMissingLastQuarter(ILogger<ReportLoader> logger, QuarterDataConfigModel lastReport)
        {
            var (controlYear, controlQuarter) = CommonHelper.QarterHelper.SubtractQuarter(DateTime.UtcNow);

            var isNew = controlYear > lastReport.Year || controlYear == lastReport.Year && controlQuarter > lastReport.Quarter;

            if (isNew)
                return isNew;

            logger.LogInformation(LogEvents.Processing, "For '{companyId}' reports is actual.", lastReport.CompanyId);
            return isNew;
        }

    }
}
