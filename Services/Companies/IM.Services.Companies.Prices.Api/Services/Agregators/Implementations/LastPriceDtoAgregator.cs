using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.Models;
using IM.Services.Companies.Prices.Api.Models.Dto;
using IM.Services.Companies.Prices.Api.Services.Agregators.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Agregators.Implementations
{
    public class LastPriceDtoAgregator : ILastPriceDtoAgregator
    {
        private readonly PricesContext context;
        public LastPriceDtoAgregator(PricesContext context) => this.context = context;

        public async Task<ResponseModel<PriceDto>> GetPriceAsync(string ticker)
        {
            var errors = Array.Empty<string>();
            string tickerName = ticker.ToUpperInvariant();

            var _ticker = await context.Tickers.FindAsync(tickerName);
            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            var price = _ticker.Prices.Where(x => x.Date >= DateTime.UtcNow.AddMonths(-1)).OrderBy(x => x.Date).LastOrDefault();
            return new()
            {
                Errors = errors,
                Data = new(price, tickerName)
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            var query = context.Tickers.AsQueryable();
            int count = await query.CountAsync();

            var prices = context.Prices
                .Where(x => x.Date >= DateTime.UtcNow.AddMonths(-1))
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.TickerName)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .ToArray();

            var lastPrices = prices
                .GroupBy(x => x.TickerName)
                .Select(x => new PriceDto(x.FirstOrDefault(), x.Key))
                .ToArray();

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
    }
}