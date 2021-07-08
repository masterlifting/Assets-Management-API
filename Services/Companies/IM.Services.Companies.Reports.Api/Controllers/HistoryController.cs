using System.Threading.Tasks;
using IM.Services.Companies.Reports.Api.Models;
using IM.Services.Companies.Reports.Api.Models.Dto;
using IM.Services.Companies.Reports.Api.Services.Agregators.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IM.Services.Companies.Reports.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryReportDtoAgregator agregator;
        public HistoryController(IHistoryReportDtoAgregator agregator) => this.agregator = agregator;
        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetReportsAsync(ticker, new(page, limit));
    }
}