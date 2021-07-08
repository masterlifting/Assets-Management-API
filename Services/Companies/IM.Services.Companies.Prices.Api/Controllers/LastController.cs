using System.Threading.Tasks;
using IM.Services.Companies.Prices.Api.Models;
using IM.Services.Companies.Prices.Api.Models.Dto;
using IM.Services.Companies.Prices.Api.Services.Agregators.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IM.Services.Companies.Prices.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class LastController : ControllerBase
    {
        private readonly ILastPriceDtoAgregator agregator;
        public LastController(ILastPriceDtoAgregator agregator) => this.agregator = agregator;

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(int page = 1, int limit = 10) =>
            await agregator.GetPricesAsync(new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PriceDto>> Get(string ticker) => await agregator.GetPriceAsync(ticker);
    }
}