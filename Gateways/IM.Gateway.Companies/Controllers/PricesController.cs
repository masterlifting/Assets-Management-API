using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Http;

using IM.Gateway.Companies.Clients;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;

namespace IM.Gateway.Companies.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly PricesClient client;
        private const string controller = "prices";
        public PricesController(PricesClient client) => this.client = client;


        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("last/")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller + "/last", GetQueryString(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.More, ticker, year, month, day), new(page, limit));
        
        [HttpGet("{ticker}/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(ticker, year), new(page, limit));
        
        [HttpGet("{ticker}/{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string ticker, int year, int month, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.Equal, ticker, year, month), new(page, limit));
        
        [HttpGet("{ticker}/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<PriceGetDto>> Get(string ticker, int year, int month, int day) =>
            await client.Get<PriceGetDto>(controller, ticker, year, month, day);


        [HttpGet("{year:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(year), new(page, limit));
        
        [HttpGet("{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.Equal, year, month), new(page, limit));
        
        [HttpGet("{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
            await client.Get<PriceGetDto>(controller, GetQueryString(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


        [HttpPost]
        public async Task<ResponseModel<string>> Post(PricePostDto model) => await client.Post(controller, model);
        
        [HttpPut("{ticker}/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int month, int day, PricePostDto model) =>
            await client.Put(controller, new PricePostDto
            {
                TickerName = ticker,
                Date = new DateTime(year, month, day),
                SourceTypeId = model.SourceTypeId,
                Value = model.Value
            });
        
        [HttpDelete("{ticker}/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Delete(string ticker, int year, int month, int day) =>
            await client.Delete(controller, ticker, year, month, day);
    }
}
