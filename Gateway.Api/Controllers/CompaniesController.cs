using System;
using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Companies;
using CommonServices.Models.Http;

using Gateway.Api.Clients;

using Microsoft.AspNetCore.Mvc;

using System.Linq;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;

namespace Gateway.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private const string tickers = "tickers";
        private const string companies = "companies";
        private const string stockSplits = "stocksplits";

        private readonly CompaniesClient companyClient;
        private readonly CompanyPricesClient pricesClient;
        private readonly CompanyReportsClient reportsClient;

        public CompaniesController(CompaniesClient companyClient, CompanyPricesClient pricesClient, CompanyReportsClient reportsClient)
        {
            this.companyClient = companyClient;
            this.pricesClient = pricesClient;
            this.reportsClient = reportsClient;
        }

        #region Companies

        public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> Get(int page = 0, int limit = 0) =>
            await companyClient.Get<CompanyGetDto>(companies, null, new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<CompanyFullDto>> Get(string ticker)
        {
            var companyResponseTask = companyClient.Get<CompanyGetDto>(companies, ticker);
            var reportTickerResponseTask = reportsClient.Get<CommonServices.Models.Dto.CompanyReports.TickerGetDto>(tickers, ticker);
            var priceTickerResponseTask = pricesClient.Get<CommonServices.Models.Dto.CompanyPrices.TickerGetDto>(tickers, ticker);

            await Task.WhenAll(companyResponseTask, reportTickerResponseTask, priceTickerResponseTask);

            var companyResponse = companyResponseTask.Result;
            if (companyResponse.Errors.Any())
                return new() { Errors = companyResponse.Errors };

            var priceTickerResponse = priceTickerResponseTask.Result;
            var reportTickerResponse = reportTickerResponseTask.Result;

            return new()
            {
                Errors = priceTickerResponse.Errors.Concat(reportTickerResponse.Errors).ToArray(),
                Data = new CompanyFullDto
                {
                    Ticker = companyResponse.Data!.Ticker,
                    Name = companyResponse.Data.Name,
                    Description = companyResponse.Data.Description,
                    Industry = companyResponse.Data.Industry,
                    Sector = companyResponse.Data.Sector,
                    PriceSource = priceTickerResponse.Data is not null
                        ? priceTickerResponse.Data.SourceName
                        : "no data",
                    ReportSource = reportTickerResponse.Data is not null
                        ? reportTickerResponse.Data.SourceName
                        : "no data",
                    ReportSourceValue = reportTickerResponse.Data?.SourceValue ?? "no data"
                }
            };
        }


        [HttpPost]
        public async Task<ResponseModel<string>> Post(CompanyPostDto company) => await companyClient.Post(companies, company);

        [HttpPut("{ticker}")]
        public async Task<ResponseModel<string>> Put(string ticker, CompanyPutDto company) => await companyClient.Put(companies, company, ticker);

        [HttpDelete("{ticker}")]
        public async Task<ResponseModel<string>> Delete(string ticker) => await companyClient.Delete(companies, ticker);
        #endregion

        #region StockSplits
        [HttpGet("stocksplits/")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("stocksplits/last/")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits + "/last", GetQueryString(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("{ticker}/stocksplits/")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.More, ticker, year, month, day), new(page, limit));

        [HttpGet("{ticker}/stocksplits/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits, GetQueryString(ticker, year), new(page, limit));

        [HttpGet("{ticker}/stocksplits/{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string ticker, int year, int month, int page = 0, int limit = 0) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.Equal, ticker, year, month), new(page, limit));

        [HttpGet("{ticker}/stocksplits/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<StockSplitGetDto>> Get(string ticker, int year, int month, int day) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits, ticker, year, month, day);


        [HttpGet("stocksplits/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits, GetQueryString(year), new(page, limit));

        [HttpGet("stocksplits/{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.Equal, year, month), new(page, limit));

        [HttpGet("stocksplits/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
            await companyClient.Get<StockSplitGetDto>(stockSplits, GetQueryString(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


        [HttpPost("stocksplits/")]
        public async Task<ResponseModel<string>> Post(StockSplitPostDto model) => await companyClient.Post(stockSplits, model);

        [HttpPut("{ticker}/stocksplits/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int month, int day, StockSplitPutDto model) =>
            await companyClient.Put(stockSplits, model, ticker, year, month, day);

        [HttpDelete("{ticker}/stocksplits/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Delete(string ticker, int year, int month, int day) =>
            await companyClient.Delete(stockSplits, ticker, year, month, day);
        #endregion
    }
}
