using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Services.Company.Analyzer.Models.Dto;
using IM.Services.Company.Analyzer.Services.DtoServices;

namespace IM.Services.Company.Analyzer.Controllers
{
    [ApiController, Route("[controller]")]
    public class RecommendationsController : Controller
    {
        private readonly RecommendationDtoAggregator agregator;
        public RecommendationsController(RecommendationDtoAggregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> Get(int page = 1, int limit = 10) => 
            await agregator.GetRecommendationsAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<RecommendationDto>> Get(string ticker) => await agregator.GetRecommendationAsync(ticker);
    }
}
