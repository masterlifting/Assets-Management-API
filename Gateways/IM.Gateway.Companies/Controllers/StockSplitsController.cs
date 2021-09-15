using CommonServices.Models.Dto.Http;

using IM.Gateway.Companies.Models.Dto;
using IM.Gateway.Companies.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Gateway.Companies.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class StockSplitsController : Controller
    {
        private readonly StockSplitDtoAggregator aggregator;
        public StockSplitsController(StockSplitDtoAggregator aggregator) => this.aggregator = aggregator;

        public async Task<ResponseModel<PaginationResponseModel<StockSplitGetDto>>> Get(int page = 0, int limit = 0) =>
            await aggregator.GetAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<StockSplitGetDto>>> Get(string ticker, int page = 0, int limit = 0) =>
            await aggregator.GetAsync(ticker, new(page, limit));
        [HttpPost]
        public async Task<ResponseModel<string>> Post(StockSplitPostDto model) => 
            await aggregator.CreateAsync(model);

        [HttpPut("{id}")]
        public async Task<ResponseModel<string>> Put(int id, StockSplitPostDto model) => 
            await aggregator.UpdateAsync(id, model);
        [HttpDelete("{id}")]
        public async Task<ResponseModel<string>> Delete(int id) => await aggregator.DeleteAsync(id);

    }
}
