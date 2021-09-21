using CommonServices.HttpServices;
using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Http;

using IM.Gateway.Companies.Clients;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Gateway.Companies.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly AnalyzerClient client;
        private const string controller = "ratings";
        public RatingsController(AnalyzerClient client) => this.client = client;

        public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> Get(int page = 0, int limit = 0) =>
            await client.Get<RatingGetDto>(controller, null, new HttpPagination(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<RatingGetDto>> Get(string ticker) => await client.Get<RatingGetDto>(controller, ticker);

        [HttpGet("{place:int}")]
        public async Task<ResponseModel<RatingGetDto>> Get(int place) => await client.Get<RatingGetDto>(controller, place);
    }
}
