using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Services.Company.Analyzer.Models.Dto;
using IM.Services.Company.Analyzer.Services.DtoServices;

namespace IM.Services.Company.Analyzer.Controllers
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
