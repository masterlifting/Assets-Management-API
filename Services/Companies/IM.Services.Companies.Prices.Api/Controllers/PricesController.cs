using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Services.Companies.Prices.Api.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class PricesController : Controller
    {
        private readonly PriceDtoAgregator agregator;
        public PricesController(PriceDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(int page = 1, int limit = 10) =>
            await agregator.GetPricesAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetPricesAsync(ticker, new(page, limit));
        
        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(string ticker, int year = 2021, int month = 1, int day = 1, int page = 1, int limit = 10) =>
            await agregator.GetPricesAsync(ticker, new(year, month, day), new(page, limit));
    }
}
