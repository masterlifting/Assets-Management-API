using System.Threading.Tasks;
using IM.Services.Companies.Reports.Api.Models;
using IM.Services.Companies.Reports.Api.Models.Dto;
using IM.Services.Companies.Reports.Api.Services.Agregators.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IM.Services.Companies.Reports.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class LastController : ControllerBase
    {
        private readonly ILastReportDtoAgregator agregator;
        public LastController(ILastReportDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> Get(int page = 1, int limit = 10) =>
            await agregator.GetReportsAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<ReportDto>> Get(string ticker) => await agregator.GetReportAsync(ticker);
    }
}