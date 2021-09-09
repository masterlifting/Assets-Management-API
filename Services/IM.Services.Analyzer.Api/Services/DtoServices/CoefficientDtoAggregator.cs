using CommonServices.Models.Dto.Http;

using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.Models.Dto;

using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.DtoServices
{
    public class CoefficientDtoAggregator
    {
        public Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination)
        {
            var result = new ResponseModel<PaginationResponseModel<CoefficientDto>>()
            {
                Errors = new string[] { "Coefficients agregator is not implemented!" }
            };

            return Task.FromResult(result);


            //var errors = Array.Empty<string>();

            //string tickerName = ticker.ToUpperInvariant();
            //var _ticker = await context.Tickers.FindAsync(tickerName);

            //if (_ticker is null)
            //    return new()
            //    {
            //        Errors = new string[] { "Ticker not found" }
            //    };

            //int count = await context.Coefficients.Where(x => x.TickerName.Equals(tickerName)).CountAsync();

            //var coefficients = Array.Empty<CoefficientDto>();

            //if (count > 0)
            //    coefficients = await context.Coefficients
            //    .OrderByDescending(x => x.Year)
            //    .ThenByDescending(x => x.Quarter)
            //    .Skip((pagination.Page - 1) * pagination.Limit)
            //    .Take(pagination.Limit)
            //    .Select(x => new CoefficientDto(x))
            //    .ToArrayAsync();

            //return new()
            //{
            //    Errors = errors,
            //    Data = new()
            //    {
            //        Items = coefficients,
            //        Count = count
            //    }
            //};
        }
    }
}
