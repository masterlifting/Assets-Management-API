
using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class RatingCalculator
    {
        private readonly AnalyzerContext context;
        private readonly AnalyzerRepository<Rating> ratingRepository;

        public RatingCalculator(AnalyzerContext context, AnalyzerRepository<Rating> ratingRepository)
        {
            this.context = context;
            this.ratingRepository = ratingRepository;
        }

        public async Task CalculateAsync()
        {
            var tickers = await context.Tickers.Select(x => x.Name).ToArrayAsync();
            var ratings = new List<Rating>(tickers.Length);

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

                ratings.Add(new()
                {
                    Place = i + 1,
                    PriceComparison = priceResult,
                    ReportComparison = reportResult,
                    Result = RatingComparator.ComputeSampleResult(new[] { priceResult, reportResult }),
                    TickerName = tickers[i]
                });
            }

            ratings.Sort((x, y) => x.Result < y.Result ? 1 : x.Result > y.Result ? -1 : 0);
            for (int i = 0; i < ratings.Count; i++)
                ratings[i].Place = i + 1;

            context.Database.ExecuteSqlRaw("DELETE FROM \"Ratings\"");

            Console.WriteLine($"ratings count: {ratings.Count}");
            await ratingRepository.CreateAsync(ratings, new RatingComparer(), "ratings");
        }
    }

    public class RatingComparer : IEqualityComparer<Rating>
    {
        public bool Equals(Rating? x, Rating? y) => x is not null && y is not null && x.TickerName == y.TickerName;
        public int GetHashCode([DisallowNull] Rating obj) => obj.TickerName.GetHashCode();
    }
}