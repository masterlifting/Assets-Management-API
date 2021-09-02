using CommonServices.Models.Dto.Http;

using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.Models.Dto;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.DtoServices
{
    public class RecommendationDtoAgregator
    {
        private readonly AnalyzerContext context;
        public RecommendationDtoAgregator(AnalyzerContext context) => this.context = context;

        public async Task<ResponseModel<RecommendationDto>> GetRecommendationAsync(string ticker)
        {
            var errors = Array.Empty<string>();

            string tickerName = ticker.ToUpperInvariant();
            var _ticker = await context.Tickers.FindAsync(tickerName);

            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            var recommendation = _ticker.Recommendation;

            if (recommendation is null)
                return new()
                {
                    Errors = new string[] { "Recommendation not found" }
                };

            return new()
            {
                Errors = errors,
                Data = new(recommendation)
            };
        }

        public async Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> GetRecommendationsAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            int count = await context.Recommendations.CountAsync();
            var recommendations = Array.Empty<RecommendationDto>();

            if (count > 0)
                recommendations = await context.Recommendations
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Select(x => new RecommendationDto(x))
                .ToArrayAsync();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = recommendations,
                    Count = count
                }
            };
        }
    }
}
