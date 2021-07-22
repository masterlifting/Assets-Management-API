using IM.Services.Companies.Reports.Api.Services.Background.ReportUpdaterBackgroundServices.Interfaces;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly IReportUpdater reportUpdater;
        public ServiceController(IReportUpdater reportUpdater) => this.reportUpdater = reportUpdater;

        [HttpPost("update")]
        public async Task<string> UpdateReports()
        {
            int updatedCount = await reportUpdater.UpdateReportsAsync();
            return $"updated reports count: {updatedCount}";
        }
    }
}