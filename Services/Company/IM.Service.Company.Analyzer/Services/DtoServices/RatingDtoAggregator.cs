using CommonServices.Models.Dto.Http;

using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Models.Dto;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.Services.DtoServices
{
    public class RatingDtoAggregator
    {
        private readonly RepositorySet<Rating> repository;
        public RatingDtoAggregator(RepositorySet<Rating> repository) => this.repository = repository;

        public async Task<ResponseModel<RatingDto>> GetAsync(string ticker)
        {

            string tickerName = ticker.ToUpperInvariant();
            var ctxTicker = await repository.GetDbSetBy<Ticker>().FindAsync(tickerName);

            if (ctxTicker is null)
                return new()
                {
                    Errors = new[] { "Ticker not found" }
                };

            var errors = Array.Empty<string>();

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

            var count = await repository.GetCountAsync();

            var paginatedResult = repository.QueryPaginator(pagination);

            var ratings = count > 0 
                ? await paginatedResult.Select(x => new RatingDto(x)).ToArrayAsync() 
                : Array.Empty<RatingDto>();

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
