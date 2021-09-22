using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Http;

using IM.Service.Company.Analyzer.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading.Tasks;
using CommonServices.HttpServices;

namespace IM.Service.Company.Analyzer.Controllers
{
    [ApiController, Route("[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly DtoRatingManager manager;
        public RatingsController(DtoRatingManager manager) => this.manager = manager;

        public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> Get(int page = 0, int limit = 0) => 
            await manager.GetAsync(new HttpPagination(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<RatingGetDto>> Get(string ticker) => await manager.GetAsync(ticker);

        [HttpGet("{place:int}")]
        public async Task<ResponseModel<RatingGetDto>> Get(int place) => await manager.GetAsync(place);

        [HttpPost("recalculate/")]
        public async Task<string> Recalculate(DateTime? dateStart = null)
        {
            dateStart ??= new DateTime(2010, 01, 01);

            var result = await manager.UpdateAsync(dateStart.Value);
            var messageResult = result ? "success" : "failed";
            return $"updated ratings {messageResult}";
        }
    }
}
