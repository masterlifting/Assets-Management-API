using CommonServices.HttpServices;
using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Http;

using Gateway.Api.Clients;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace Gateway.Api.Controllers
{
    [ApiController, Route("api/companies")]
    public class CompanyAnalyzerController : ControllerBase
    {
        private readonly CompanyAnalyzerClient client;
        private const string rating = "ratings";
        public CompanyAnalyzerController(CompanyAnalyzerClient client) => this.client = client;

        [HttpGet("ratings/")]
        public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> Get(int page = 0, int limit = 0) =>
            await client.Get<RatingGetDto>(rating, null, new HttpPagination(page, limit));

        [HttpGet("{ticker}/ratings/")]
        public async Task<ResponseModel<RatingGetDto>> Get(string ticker) => await client.Get<RatingGetDto>(rating, ticker);

        [HttpGet("ratings/{place:int}")]
        public async Task<ResponseModel<RatingGetDto>> Get(int place) => await client.Get<RatingGetDto>(rating, place);
    }
}
