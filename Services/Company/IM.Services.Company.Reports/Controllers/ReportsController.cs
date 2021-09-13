using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Services.Company.Reports.Services.DtoServices;

namespace IM.Services.Company.Reports.Controllers
{
    [ApiController, Route("[controller]")]
    public class ReportsController : Controller
    {
        private readonly ReportsDtoAggregator aggregator;
        public ReportsController(ReportsDtoAggregator aggregator) => this.aggregator = aggregator;

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(int year = 0, byte quarter = 0, int page = 0, int limit = 0) =>
            await aggregator.GetReportsAsync(new(year, quarter), new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(string ticker, int year = 0, byte quarter = 0, int page = 0, int limit = 0) =>
            await aggregator.GetReportsAsync(ticker, new(year, quarter), new(page, limit));
    }
}
