
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using CommonServices.HttpServices;
using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Http;

namespace IM.Service.Company.Analyzer.Services.DtoServices
{
    public class DtoRatingManager
    {
        private readonly RepositorySet<Rating> repository;
        private readonly RatingCalculator ratingCalculator;
        private readonly ReportCalculator reportCalculator;
        private readonly PriceCalculator priceCalculator;

        public DtoRatingManager(
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

        public async Task<ResponseModel<RatingGetDto>> GetAsync(int place)
        {
            var rating = await repository.FindAsync(place);

            if (rating is null)
                return new()
                {
                    Errors = new[] { "rating not found" }
                };

            var errors = Array.Empty<string>();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Ticker = rating.TickerName,
                    Place = rating.Place,
                    PriceComparison = rating.PriceComparison,
                    ReportComparison = rating.ReportComparison,
                    Result = rating.Result,
                    UpdateTime = rating.UpdateTime
                }
            };
        }
        public async Task<ResponseModel<RatingGetDto>> GetAsync(string ticker)
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
                Data = new()
                {
                    Ticker = rating.TickerName,
                    Place = rating.Place,
                    PriceComparison = rating.PriceComparison,
                    ReportComparison = rating.ReportComparison,
                    Result = rating.Result,
                    UpdateTime = rating.UpdateTime
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetAsync(HttpPagination pagination)
        {
            var errors = Array.Empty<string>();

            var count = await repository.GetCountAsync();

            var paginatedResult = repository.GetPaginationQuery(pagination);

            var ratings = count > 0
                ? await paginatedResult.Select(x => new RatingGetDto
                {
                    Ticker = x.TickerName,
                    Place = x.Place,
                    PriceComparison = x.PriceComparison,
                    ReportComparison = x.ReportComparison,
                    Result = x.Result,
                    UpdateTime = x.UpdateTime
                }).ToArrayAsync()
                : Array.Empty<RatingGetDto>();

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
