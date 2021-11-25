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
            var (controlYear, controlQuarter) = CommonHelper.SubtractQuarter(DateTime.UtcNow);

            var isNew = controlYear > lastReport.Year || controlYear == lastReport.Year && controlQuarter > lastReport.Quarter;

            if (isNew)
                return isNew;

            logger.LogInformation(LogEvents.Processing, "last quarter is actual for '{companyId}'. Report will not be loaded.", lastReport.CompanyId);
            return isNew;
        }

    }
}
