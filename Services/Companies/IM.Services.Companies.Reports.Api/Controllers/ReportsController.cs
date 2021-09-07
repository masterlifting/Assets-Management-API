using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Services.Companies.Reports.Api.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class ReportsController : Controller
    {
        private readonly ReportsDtoAgregator agregator;
        public ReportsController(ReportsDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(int page = 1, int limit = 10) =>
            await agregator.GetReportsAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetReportsAsync(ticker, new(page, limit));

        [HttpGet("{ticker}/filter/")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(string ticker, int year, byte quarter, int page = 1, int limit = 10) =>
          await agregator.GetReportsAsync(ticker, new(year, quarter), new(page, limit));
    }
}
