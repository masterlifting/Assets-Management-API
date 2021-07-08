using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Models.Http;
using IM.Services.Analyzer.Api.Models.Dto;
using IM.Services.Analyzer.Api.Services.Agregators.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.Agregators.Implementations
{
    public class RatingDtoAgregator : IRatingDtoAgregator
    {
        private readonly AnalyzerContext context;
        public RatingDtoAgregator(AnalyzerContext context) => this.context = context;

        public async Task<ResponseModel<RatingDto>> GetRatingAsync(string ticker)
        {
            var errors = Array.Empty<string>();

            string tickerName = ticker.ToUpperInvariant();
            var _ticker = await context.Tickers.FindAsync(tickerName);

            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            var rating = _ticker.Rating;

            if (rating is null)
                return new()
                {
                    Errors = new string[] { "Rating not found" }
                };

            return new()
            {
                Errors = errors,
                Data = new(rating)
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetRatingsAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            int count = await context.Ratings.CountAsync();
            var ratings = Array.Empty<RatingDto>();

            if (count > 0)
                ratings = await context.Ratings
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Select(x => new RatingDto(x))
                .ToArrayAsync();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = ratings,
                    Count = count
                }
            };
        }
    }
}
