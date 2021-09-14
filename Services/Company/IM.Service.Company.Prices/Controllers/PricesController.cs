using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Service.Company.Prices.Services.DtoServices;

namespace IM.Service.Company.Prices.Controllers
{
    [ApiController, Route("[controller]")]
    public class PricesController : Controller
    {
        private readonly PriceDtoAggregator aggregator;
        public PricesController(PriceDtoAggregator aggregator) => this.aggregator = aggregator;

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await aggregator.GetPricesAsync(new(year, month, day), new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await aggregator.GetPricesAsync(ticker, new(year, month, day), new(page, limit));
    }
}
