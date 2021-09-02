using CommonServices.Models.Dto.Http;

using IM.Services.Analyzer.Api.Models.Dto;
using IM.Services.Analyzer.Api.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class CoefficientsController : Controller
    {
        private readonly CoefficientDtoAgregator agregator;
        public CoefficientsController(CoefficientDtoAgregator agregator) => this.agregator = agregator;

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetCoefficientsAsync(ticker, new(page, limit));
    }
}
