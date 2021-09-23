using CommonServices.Models.Dto.GatewayCompanies;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;
using IM.Gateway.Companies.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;

namespace IM.Gateway.Companies.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class StockSplitsController : ControllerBase
    {
        private readonly DtoStockSplitManager manager;
        public StockSplitsController(DtoStockSplitManager manager) => this.manager = manager;

        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("last/")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetLastAsync(new(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(HttpRequestFilterType.More, ticker, year, month, day), new(page, limit));
        
        [HttpGet("{ticker}/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(ticker, year), new(page, limit));
        
        [HttpGet("{ticker}/{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string ticker, int year, int month, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(ticker, year, month), new(page, limit));
        
        [HttpGet("{ticker}/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<StockSplitGetDto>> Get(string ticker, int year, int month, int day) =>
            await manager.GetAsync(new PriceIdentity() { TickerName = ticker, Date = new DateTime(year, month, day) });


        [HttpGet("{year:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(year), new(page, limit));
        
        [HttpGet("{year:int}/{month:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(year, month), new(page, limit));
        
        [HttpGet("{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


        [HttpPost]
        public async Task<ResponseModel<string>> Post(StockSplitPostDto model) => await manager.CreateAsync(model);
        
        [HttpPut("{ticker}/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int month, int day, StockSplitPutDto model) =>
            await manager.UpdateAsync(new StockSplitPostDto
            {
                TickerName = ticker,
                Date = new DateTime(year, month, day),
                Value = model.Value
            });
        
        [HttpDelete("{ticker}/{year:int}/{month:int}/{day:int}")]
        public async Task<ResponseModel<string>> Delete(string ticker, int year, int month, int day) =>
            await manager.DeleteAsync(new PriceIdentity { TickerName = ticker, Date = new DateTime(year, month, day) });
    }
}
