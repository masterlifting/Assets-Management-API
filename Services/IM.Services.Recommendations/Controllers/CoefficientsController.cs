using CommonServices.Models.Dto.Http;

using IM.Services.Recommendations.Models.Dto;
using IM.Services.Recommendations.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Recommendations.Controllers
{
    [ApiController, Route("[controller]")]
    public class CoefficientsController : Controller
    {
        private readonly CoefficientDtoAggregator agregator;
        public CoefficientsController(CoefficientDtoAggregator agregator) => this.agregator = agregator;

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetCoefficientsAsync(ticker, new(page, limit));
    }
}
