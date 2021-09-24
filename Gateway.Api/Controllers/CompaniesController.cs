using CommonServices.Models.Dto.GatewayCompanies;
using CommonServices.Models.Http;

using Gateway.Api.Clients;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;

namespace Gateway.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly CompaniesClient client;
        private const string companies = "companies";
        private const string stockSplits = "stocksplits";
        public CompaniesController(CompaniesClient client) => this.client = client;

        #region Companies
        public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> Get(int page = 0, int limit = 0) =>
            await client.Get<CompanyGetDto>(companies, null, new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<CompanyGetDto>> Get(string ticker) => await client.Get<CompanyGetDto>(companies, ticker);


        [HttpPost]
        public async Task<ResponseModel<string>> Post(CompanyPostDto company) => await client.Post(companies, company);

        [HttpPut("{ticker}")]
        public async Task<ResponseModel<string>> Put(string ticker, CompanyPutDto company) => await client.Put(companies, company, ticker);

        [HttpDelete("{ticker}")]
        public async Task<ResponseModel<string>> Delete(string ticker) => await client.Delete(companies, ticker);
        #endregion

        #region StockSplits
        [HttpGet("stocksplits/")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("stocksplits/last/")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<StockSplitGetDto>(stockSplits + "/last", GetQueryString(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("{ticker}/stocksplits/")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await client.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.More, ticker, year, month, day), new(page, limit));

        [HttpGet("{ticker}/stocksplits/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await client.Get<StockSplitGetDto>(stockSplits, GetQueryString(ticker, year), new(page, limit));

        [HttpGet("{ticker}/stocksplits/{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string ticker, int year, int month, int page = 0, int limit = 0) =>
            await client.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.Equal, ticker, year, month), new(page, limit));

        [HttpGet("{ticker}/stocksplits/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<StockSplitGetDto>> Get(string ticker, int year, int month, int day) =>
            await client.Get<StockSplitGetDto>(stockSplits, ticker, year, month, day);


        [HttpGet("stocksplits/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await client.Get<StockSplitGetDto>(stockSplits, GetQueryString(year), new(page, limit));

        [HttpGet("stocksplits/{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
            await client.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.Equal, year, month), new(page, limit));

        [HttpGet("stocksplits/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
            await client.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


        [HttpPost("stocksplits/")]
        public async Task<ResponseModel<string>> Post(StockSplitPostDto model) => await client.Post(stockSplits, model);

        [HttpPut("{ticker}/stocksplits/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int month, int day, StockSplitPutDto model) =>
            await client.Put(stockSplits, model,  ticker,  year, month, day);

        [HttpDelete("{ticker}/stocksplits/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Delete(string ticker, int year, int month, int day) =>
            await client.Delete(stockSplits, ticker, year, month, day);
        #endregion
    }
}
