using System.Threading.Tasks;
using IM.Services.Companies.Prices.Api.Models;
using IM.Services.Companies.Prices.Api.Models.Dto;
using IM.Services.Companies.Prices.Api.Services.Agregators.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IM.Services.Companies.Prices.Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryPriceDtoAgregator agregator;
        public HistoryController(IHistoryPriceDtoAgregator agregator) => this.agregator = agregator;
        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> Get(string ticker, int page = 1, int limit = 10) =>
            await agregator.GetPricesAsync(ticker, new(page, limit));
    }
}