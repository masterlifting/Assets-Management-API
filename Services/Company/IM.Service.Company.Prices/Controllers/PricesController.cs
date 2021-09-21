using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;

using IM.Service.Company.Prices.Services.DtoServices;
using IM.Service.Company.Prices.Services.PriceServices;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;

namespace IM.Service.Company.Prices.Controllers
{
    [ApiController, Route("[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly DtoManager manager;
        private readonly PriceLoader loader;
        public PricesController(DtoManager manager, PriceLoader loader)
        {
            this.manager = manager;
            this.loader = loader;
        }

        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("last/")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetLastAsync(new(HttpRequestFilterType.More, year, month, day), new(page, limit));

        [HttpGet("{Ticker}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(string ticker, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(HttpRequestFilterType.More, ticker, year, month, day), new(page, limit));
        
        [HttpGet("{Ticker}/{Year:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(ticker, year), new(page, limit));
        
        [HttpGet("{Ticker}/{Year:int}/{Month:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string ticker, int year, int month, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(ticker, year, month), new(page, limit));
        
        [HttpGet("{Ticker}/{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<PriceGetDto>> Get(string ticker, int year, int month, int day) =>
            await manager.GetAsync(new PriceIdentity { TickerName = ticker, Date = new DateTime(year, month, day) });


        [HttpGet("{Year:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(year), new(page, limit));
        
        [HttpGet("{Year:int}/{Month:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(year, month), new(page, limit));
        
        [HttpGet("{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


        [HttpPost]
        public async Task<ResponseModel<string>> Post(PricePostDto model) => await manager.CreateAsync(model);
        [HttpPut("{Ticker}/{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int month, int day, PricePostDto model) =>
            await manager.UpdateAsync(new PricePostDto
            {
                TickerName = ticker.ToUpperInvariant(),
                Date = new DateTime(year, month, day),
                SourceTypeId = model.SourceTypeId,
                Value = model.Value
            });
        [HttpDelete("{Ticker}/{Year:int}/{Month:int}/{Day:int}")]
        public async Task<ResponseModel<string>> Delete(string ticker, int year, int month, int day) =>
            await manager.DeleteAsync(new PriceIdentity { TickerName = ticker, Date = new DateTime(year, month, day) });

        [HttpPost("load/")]
        public async Task<string> Load()
        {
            var loadedCount = await loader.LoadAsync();
            return $"loaded prices count: {loadedCount}";
        }
    }
}
