using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.Models.Dto;
using IM.Services.Analyzer.Api.Models.Http;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.DtoServices
{
    public class CoefficientDtoAgregator
    {
        private readonly AnalyzerContext context;
        public CoefficientDtoAgregator(AnalyzerContext context) => this.context = context;

        public async Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            string tickerName = ticker.ToUpperInvariant();
            var _ticker = await context.Tickers.FindAsync(tickerName);

            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            int count = await context.Coefficients.Where(x => x.TickerName.Equals(tickerName)).CountAsync();

            var coefficients = Array.Empty<CoefficientDto>();

            if (count > 0)
                coefficients = await context.Coefficients
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Quarter)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Select(x => new CoefficientDto(x))
                .ToArrayAsync();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = coefficients,
                    Count = count
                }
            };
        }
    }
}
