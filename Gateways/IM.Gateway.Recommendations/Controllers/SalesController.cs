using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Gateway.Recommendations.Models.Dto;
using IM.Gateway.Recommendations.Services.DtoServices;

namespace IM.Gateway.Recommendations.Controllers
{
    [ApiController, Route("[controller]")]
    public class SalesController : Controller
    {
        private readonly SaleDtoAggregator agregator;
        public SalesController(SaleDtoAggregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> Get(int page = 1, int limit = 10) => 
            await agregator.GetRecommendationsAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<RecommendationDto>> Get(string ticker) => await agregator.GetRecommendationAsync(ticker);
    }
}
