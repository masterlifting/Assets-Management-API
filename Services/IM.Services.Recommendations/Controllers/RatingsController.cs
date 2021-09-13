using CommonServices.Models.Dto.Http;

using IM.Services.Recommendations.Models.Dto;
using IM.Services.Recommendations.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Recommendations.Controllers
{
    [ApiController, Route("[controller]")]
    public class RatingsController : Controller
    {
        private readonly RatingDtoAggregator agregator;
        public RatingsController(RatingDtoAggregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<RatingDto>>> Get(int page = 1, int limit = 10) => 
            await agregator.GetRatingsAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<RatingDto>> Get(string ticker) => await agregator.GetRatingAsync(ticker);
    }
}
