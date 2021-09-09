using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Services.Companies.Prices.Api.DataAccess;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.DtoServices
{
    public class PriceDtoAggregator
    {
        private readonly PricesContext context;
        public PriceDtoAggregator(PricesContext context) => this.context = context;

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var prices = context.Prices.Where(x => x.Date.Year > filter.Year || x.Date.Year == filter.Year && (x.Date.Month == filter.Month && x.Date.Day >= filter.Day || x.Date.Month > filter.Month));
            var count = await prices.CountAsync();
            var result = await prices
                .OrderBy(x => x.Date)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) =>
                    new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
                .ToArrayAsync();

            var groupedResult = result.GroupBy(x => x.TickerName).Select(x => x.First()).ToArray();

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    Items = groupedResult,
                    Count = groupedResult.Length
                }
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            var tickerName = ticker.ToUpperInvariant();
            var ctxTicker = await context.Tickers.FindAsync(tickerName);

            if (ctxTicker is null)
                return new()
                {
                    Errors = new[] { "Ticker not found" }
                };

            var prices = context.Prices.Where(x => x.TickerName == ctxTicker.Name && (x.Date.Year > filter.Year || x.Date.Year == filter.Year && (x.Date.Month == filter.Month && x.Date.Day >= filter.Day || x.Date.Month > filter.Month)));
            
            var count = await prices.CountAsync();

            var result = await prices
                .OrderBy(x => x.Date)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => 
                    new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
                .ToArrayAsync();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = result,
                    Count = count
                }
            };
        }
    }
}
