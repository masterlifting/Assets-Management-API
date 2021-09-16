using CommonServices.Models.Dto.Http;

using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Models.Dto;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.Services.CalculatorServices;

namespace IM.Service.Company.Analyzer.Services.DtoServices
{
    public class RatingDtoAggregator
    {
        private readonly RepositorySet<Rating> repository;
        private readonly RatingCalculator ratingCalculator;
        private readonly ReportCalculator reportCalculator;
        private readonly PriceCalculator priceCalculator;

        public RatingDtoAggregator(
            RepositorySet<Rating> repository
            , RatingCalculator ratingCalculator
            , ReportCalculator reportCalculator
            , PriceCalculator priceCalculator)
        {
            this.repository = repository;
            this.ratingCalculator = ratingCalculator;
            this.reportCalculator = reportCalculator;
            this.priceCalculator = priceCalculator;
        }

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
        public async Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetAsync(PaginationRequestModel pagination)
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

        public async Task<bool> UpdateAsync(DateTime dateStart)
        {
            try
            {
                if (await priceCalculator.CalculateAsync(dateStart))
                    if (await reportCalculator.CalculateAsync(dateStart))
                        await ratingCalculator.CalculateAsync();
                    else
                        await ratingCalculator.CalculateAsync();
                else if (await reportCalculator.CalculateAsync(dateStart))
                    await ratingCalculator.CalculateAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Analyzer exception: {ex.InnerException?.Message ?? ex.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;
                
                return false;
            }
        }
    }
}
