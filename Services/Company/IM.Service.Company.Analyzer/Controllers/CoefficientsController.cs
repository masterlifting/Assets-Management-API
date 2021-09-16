using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Service.Company.Analyzer.Models.Dto;
using IM.Service.Company.Analyzer.Services.DtoServices;

namespace IM.Service.Company.Analyzer.Controllers
{
    [ApiController, Route("[controller]")]
    public class CoefficientsController : Controller
    {
        private readonly CoefficientDtoAggregator aggregator;
        public CoefficientsController(CoefficientDtoAggregator aggregator) => this.aggregator = aggregator;

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await aggregator.GetCoefficientsAsync(ticker, new(page, limit));
    }
}
