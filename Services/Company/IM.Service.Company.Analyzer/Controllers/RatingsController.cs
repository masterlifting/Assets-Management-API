using System;
using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Service.Company.Analyzer.Models.Dto;
using IM.Service.Company.Analyzer.Services.DtoServices;

namespace IM.Service.Company.Analyzer.Controllers
{
    [ApiController, Route("[controller]")]
    public class RatingsController : Controller
    {
        private readonly RatingDtoAggregator aggregator;
        public RatingsController(RatingDtoAggregator aggregator) => this.aggregator = aggregator;

        public async Task<ResponseModel<PaginationResponseModel<RatingDto>>> Get(int page = 1, int limit = 10) => 
            await aggregator.GetAsync((PaginationRequestModel) new(page, limit));
        [HttpGet("{ticker}")]
        public async Task<ResponseModel<RatingDto>> Get(string ticker) => await aggregator.GetAsync(ticker);
        [HttpPost("update")]
        public async Task<string> Update(DateTime? dateStart = null)
        {
            dateStart ??= new DateTime(2019, 01, 01);

            var result = await aggregator.UpdateAsync(dateStart.Value);
            var messageResult = result ? "success" : "failed";
            return $"updated ratings {messageResult}";
        }
    }
}
