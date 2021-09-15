using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Service.Company.Reports.Services.DtoServices;
using IM.Service.Company.Reports.Services.ReportServices;

namespace IM.Service.Company.Reports.Controllers
{
    [ApiController, Route("[controller]")]
    public class ReportsController : Controller
    {
        private readonly ReportsDtoAggregator aggregator;
        private readonly ReportLoader reportUpdater;

        public ReportsController(ReportsDtoAggregator aggregator, ReportLoader reportUpdater)
        {
            this.aggregator = aggregator;
            this.reportUpdater = reportUpdater;
        }

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(int year = 0, byte quarter = 0, int page = 0, int limit = 0) =>
            await aggregator.GetReportsAsync(new(year, quarter), new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(string ticker, int year = 0, byte quarter = 0, int page = 0, int limit = 0) =>
            await aggregator.GetReportsAsync(ticker, new(year, quarter), new(page, limit));

        [HttpPost("update/")]
        public async Task<string> UpdateReports()
        {
            var loadedReports = await reportUpdater.LoadReportsAsync();
            return $"loaded reports count: {loadedReports.Length}";
        }
    }
}
