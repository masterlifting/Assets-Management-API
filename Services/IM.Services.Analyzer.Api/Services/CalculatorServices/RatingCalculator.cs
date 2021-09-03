using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class RatingCalculator
    {
        private readonly AnalyzerContext context;
        private readonly EntityRepository<Rating, AnalyzerContext> ratingRepository;

        public RatingCalculator(AnalyzerContext context, EntityRepository<Rating, AnalyzerContext> ratingRepository)
        {
            this.context = context;
            this.ratingRepository = ratingRepository;
        }

        public async Task CalculateAsync()
        {
            var tickers = await context.Tickers.Select(x => x.Name).ToArrayAsync();
            var ratings = new Rating[tickers.Length];

            for (int i = 0; i < tickers.Length; i++)
            {
                var priceResults = await context.Prices
                    .Where(x => x.TickerName == tickers[i] && x.StatusId == (byte)StatusType.Calculated)
                    .Select(x => x.Result)
                    .ToArrayAsync();

                var reportResults = await context.Reports
                    .Where(x => x.TickerName == tickers[i] && x.StatusId == (byte)StatusType.Calculated)
                    .Select(x => x.Result)
                    .ToArrayAsync();

                decimal priceResult = RatingComparator.ComputeSampleResult(priceResults);
                decimal reportResult = RatingComparator.ComputeSampleResult(reportResults);

                ratings[i] = new()
                {
                    Place = i,
                    PriceComparison = priceResult,
                    ReportComparison = reportResult,
                    Result = (priceResult + reportResult) * 0.5m,
                    TickerName = tickers[i]
                };
            }

            var sortedRatings = ratings.OrderByDescending(x => x.Result).ToArray();

            for (int i = 0; i < sortedRatings.Length; i++)
            {
                sortedRatings[i].Place = i + 1;
                await ratingRepository.CreateOrUpdateAsync(sortedRatings[i], $"rating for {sortedRatings[i].TickerName}. Place: {sortedRatings[i].Place}");
            }
        }
    }
}
