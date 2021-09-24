using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Http;

using Gateway.Api.Clients;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;

namespace Gateway.Api.Controllers
{
    [ApiController, Route("api/companies")]
    public class CompanyPricesController : ControllerBase
    {
        private readonly CompanyPricesClient client;
        private const string controller = "prices";
        public CompanyPricesController(CompanyPricesClient client) => this.client = client;

        [HttpGet("prices/")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("prices/last/")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller + "/last", GetQueryString(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("{ticker}/prices/")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.More, ticker, year, month, day), new(page, limit));
        
        [HttpGet("{ticker}/prices/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(ticker, year), new(page, limit));
        
        [HttpGet("{ticker}/prices/{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string ticker, int year, int month, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.Equal, ticker, year, month), new(page, limit));
        
        [HttpGet("{ticker}/prices/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<PriceGetDto>> Get(string ticker, int year, int month, int day) =>
            await client.Get<PriceGetDto>(controller, ticker, year, month, day);


        [HttpGet("prices/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(year), new(page, limit));
        
        [HttpGet("prices/{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.Equal, year, month), new(page, limit));
        
        [HttpGet("prices/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


        [HttpPost("prices/")]
        public async Task<ResponseModel<string>> Post(PricePostDto model) => await client.Post(controller, model);
        
        [HttpPut("{ticker}/prices/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int month, int day, PricePutDto model) =>
            await client.Put(controller, model, ticker, year, month, day);

        [HttpDelete("{ticker}/prices/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Delete(string ticker, int year, int month, int day) =>
            await client.Delete(controller, ticker, year, month, day);
    }
}
