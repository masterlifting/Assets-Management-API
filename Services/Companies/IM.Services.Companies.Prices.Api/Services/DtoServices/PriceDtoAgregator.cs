using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.Models;
using IM.Services.Companies.Prices.Api.Models.Dto;

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
                .Join(query, x => x.TickerName, y => y.Name, (x, y) => new PriceDto(x, y.PriceSourceTypeId, x.TickerName))
                .ToArrayAsync();

            var lastPrices = prices.GroupBy(x => x.Ticker).Select(x => x.First()).ToArray();

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
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new PriceDto(x, y.PriceSourceTypeId, x.TickerName))
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
