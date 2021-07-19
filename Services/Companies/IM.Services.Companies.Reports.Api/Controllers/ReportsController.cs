using IM.Services.Companies.Reports.Api.Models;
using IM.Services.Companies.Reports.Api.Models.Dto;
using IM.Services.Companies.Reports.Api.Services.Agregators.Interfaces;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class ReportsController : Controller
    {
        private readonly IReportsDtoAgregator agregator;
        public ReportsController(IReportsDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(int page = 1, int limit = 10) =>
            await agregator.GetReportsAsync(new(page, limit));
        
        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetReportsAsync(ticker, new(page, limit));
    }
}
