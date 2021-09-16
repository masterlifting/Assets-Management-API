using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Service.Company.Prices.Services.DtoServices;
using IM.Service.Company.Prices.Services.PriceServices;

namespace IM.Service.Company.Prices.Controllers
{
    [ApiController, Route("[controller]")]
    public class PricesController : Controller
    {
        private readonly PriceDtoAggregator aggregator;
        private readonly PriceLoader priceLoader;

        public PricesController(PriceDtoAggregator aggregator, PriceLoader priceLoader)
        {
            this.aggregator = aggregator;
            this.priceLoader = priceLoader;
        }

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await aggregator.GetAsync(new(year, month, day), new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await aggregator.GetAsync(ticker, new(year, month, day), new(page, limit));
        
        [HttpPost("update/")]
        public async Task<string> Update()
        {
            var loadedCount = await priceLoader.LoadPricesAsync();
            return $"loaded prices count: {loadedCount}";
        }
    }
}
