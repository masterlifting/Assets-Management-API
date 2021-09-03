using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Services.Companies.Prices.Api.DataAccess;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.DtoServices
{
    public class PriceDtoAgregator
    {
        private readonly PricesContext context;
        public PriceDtoAgregator(PricesContext context) => this.context = context;

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            var query = context.Tickers.AsQueryable();
            int count = await query.CountAsync();

            var prices = await context.Prices
                .Where(x => x.Date >= DateTime.UtcNow.AddMonths(-1))
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.TickerName)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(query, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
                .ToArrayAsync();

            var lastPrices = prices.GroupBy(x => x.TickerName).Select(x => x.First()).ToArray();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = lastPrices,
                    Count = count
                }
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            string tickerName = ticker.ToUpperInvariant();
            var _ticker = await context.Tickers.FindAsync(tickerName);

            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            int count = _ticker.Prices.Count();
            var monthCount = (int)Math.Ceiling((decimal)(pagination.Page * pagination.Limit) / 20);

            var tickerPrices = _ticker.Prices
                .Where(x => x.Date >= DateTime.UtcNow.AddMonths(-monthCount))
                .OrderByDescending(x => x.Date)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId, })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
                .ToArray();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = tickerPrices,
                    Count = count
                }
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            string tickerName = ticker.ToUpperInvariant();
            var _ticker = await context.Tickers.FindAsync(tickerName);

            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            int count = _ticker.Prices.Count();
            var tickerPrices = _ticker.Prices
                .Where(x => filter.FilterDate(x.Date.Year, x.Date.Month, x.Date.Day))
                .OrderByDescending(x => x.Date)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
                .ToArray();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = tickerPrices,
                    Count = count
                }
            };
        }
    }
}
