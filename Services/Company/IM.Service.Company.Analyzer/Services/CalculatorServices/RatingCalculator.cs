using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.DataAccess;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices
{
    public class RatingCalculator
    {
        private readonly DatabaseContext context;
        private readonly RepositorySet<Rating> ratingRepository;

        public RatingCalculator(DatabaseContext context, RepositorySet<Rating> ratingRepository)
        {
            this.context = context;
            this.ratingRepository = ratingRepository;
        }

        public async Task CalculateAsync()
        {
            var tickers = await context.Tickers.Select(x => x.Name).ToArrayAsync();
            var ratings = new List<Rating>(tickers.Length);

            for (var i = 0; i < tickers.Length; i++)
            {
                var priceResults = await context.Prices
                    .Where(x => x.TickerName == tickers[i] && x.StatusId == (byte)Enums.StatusType.Calculated)
                    .Select(x => x.Result)
                    .ToArrayAsync();

                var reportResults = await context.Reports
                    .Where(x => x.TickerName == tickers[i] && x.StatusId == (byte)Enums.StatusType.Calculated)
                    .Select(x => x.Result)
                    .ToArrayAsync();

                var priceResult = RatingComparator.ComputeSampleResult(priceResults);
                var reportResult = RatingComparator.ComputeSampleResult(reportResults);

                ratings.Add(new()
                {
                    Place = i + 1,
                    PriceComparison = priceResult,
                    ReportComparison = reportResult,
                    Result = (priceResult + reportResult) * 0.5m,
                    TickerName = tickers[i]
                });
            }

            ratings.Sort((x, y) => x.Result < y.Result ? 1 : x.Result > y.Result ? -1 : 0);

            for (var i = 0; i < ratings.Count; i++)
                ratings[i].Place = i + 1;

            await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Ratings\"");
            await ratingRepository.CreateAsync(ratings, new RatingComparer(), $"ratings count {ratings.Count}");
        }
    }

    public class RatingComparer : IEqualityComparer<Rating>
    {
        public bool Equals(Rating? x, Rating? y) => x is not null && y is not null && x.TickerName == y.TickerName;
        public int GetHashCode(Rating obj) => obj.TickerName.GetHashCode();
    }
}