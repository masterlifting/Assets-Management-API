using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.Models;
using IM.Services.Companies.Prices.Api.Models.Dto;
using IM.Services.Companies.Prices.Api.Services.Agregators.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Agregators.Implementations
{
    public class HistoryPriceDtoAgregator : IHistoryPriceDtoAgregator
    {
        private readonly PricesContext context;
        public HistoryPriceDtoAgregator(PricesContext context) => this.context = context;

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
                .Select(x => new PriceDto(x, tickerName))
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