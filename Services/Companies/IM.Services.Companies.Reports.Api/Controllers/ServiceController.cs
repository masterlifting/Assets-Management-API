using IM.Services.Companies.Reports.Api.Services.ReportServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly ReportLoader reportUpdater;
        public ServiceController(ReportLoader reportUpdater) => this.reportUpdater = reportUpdater;

        [HttpPost("update")]
        public async Task<string> UpdateReports()
        {
            var loadedReports = await reportUpdater.LoadReportsAsync();
            return $"updated reports count: {loadedReports.Length}";
        }
    }
}