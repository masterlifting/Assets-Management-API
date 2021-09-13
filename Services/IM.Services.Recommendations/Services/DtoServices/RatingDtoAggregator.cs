﻿using CommonServices.Models.Dto.Http;

using IM.Services.Recommendations.DataAccess;
using IM.Services.Recommendations.Models.Dto;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Recommendations.Services.DtoServices
{
    public class RatingDtoAggregator
    {
        private readonly AnalyzerContext context;
        public RatingDtoAggregator(AnalyzerContext context) => this.context = context;

        public async Task<ResponseModel<RatingDto>> GetRatingAsync(string ticker)
        {
            var errors = Array.Empty<string>();

            string tickerName = ticker.ToUpperInvariant();
            var ctxTicker = await context.Tickers.FindAsync(tickerName);

            if (ctxTicker is null)
                return new()
                {
                    Errors = new[] { "Ticker not found" }
                };

            var rating = ctxTicker.Rating;

            return new()
            {
                Errors = errors,
                Data = new(rating)
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetRatingsAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            var count = await context.Ratings.CountAsync();
            var ratings = Array.Empty<RatingDto>();

            if (count > 0)
                ratings = await context.Ratings
                .OrderBy(x => x.Place)
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