using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices
{
    public class RatingCalculator
    {
        private readonly RepositorySet<Ticker> tickerRepository;
        private readonly RepositorySet<Rating> ratingRepository;
        private readonly RepositorySet<Price> priceRepository;
        private readonly RepositorySet<Report> reportsRepository;

        public RatingCalculator(
            RepositorySet<Ticker> tickerRepository,
            RepositorySet<Rating> ratingRepository,
            RepositorySet<Price> priceRepository,
            RepositorySet<Report> reportsRepository)
        {
            this.tickerRepository = tickerRepository;
            this.ratingRepository = ratingRepository;
            this.priceRepository = priceRepository;
            this.reportsRepository = reportsRepository;
        }

        public async Task CalculateAsync()
        {
            var tickers = await tickerRepository.GetSampleAsync(x => x.Name);
            var ratings = new List<Rating>(tickers.Length);

            for (var i = 0; i < tickers.Length; i++)
            {
                var index = i;

                var priceResults = await priceRepository.GetSampleAsync(
                    x => x.TickerName == tickers[index] && x.StatusId == (byte)StatusType.Calculated,
                    x => x.Result);

                var reportResults = await reportsRepository.GetSampleAsync(
                    x => x.TickerName == tickers[index] && x.StatusId == (byte)StatusType.Calculated,
                    x => x.Result);

                var priceResult = RatingComparator.ComputeSampleResult(priceResults) * 100;
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

            
            await ratingRepository.DeleteAsync("ratings");
            
            await ratingRepository.CreateAsync(ratings, new RatingComparer(), $"ratings count {ratings.Count}");
        }
    }

    public class RatingComparer : IEqualityComparer<Rating>
    {
        public bool Equals(Rating? x, Rating? y) => x is not null && y is not null && x.TickerName == y.TickerName;
        public int GetHashCode(Rating obj) => obj.TickerName.GetHashCode();
    }
}