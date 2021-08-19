using IM.Services.Analyzer.Api.Models.Dto;
using IM.Services.Analyzer.Api.Models.Http;
using IM.Services.Analyzer.Api.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class RatingsController : Controller
    {
        private readonly RatingDtoAgregator agregator;
        public RatingsController(RatingDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<RatingDto>>> Get(int page = 1, int limit = 10) => await agregator.GetRatingsAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<RatingDto>> Get(string ticker) => await agregator.GetRatingAsync(ticker);
    }
}
