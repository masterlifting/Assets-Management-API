using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess.Entities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class RatingRepositoryHandler : IRepository<Rating>
    {
        private readonly AnalyzerContext context;
        public RatingRepositoryHandler(AnalyzerContext context) => this.context = context;

        public bool TryCheckEntity(Rating entity, out Rating? result)
        {
            result = null;
            var isValid = context.Tickers.Where(x => x.Name.Equals(entity.TickerName)).Any();

            if (isValid)
                result = entity;

            return isValid;
        }
        public bool TryCheckEntities(IEnumerable<Rating> entities, out Rating[] result)
        {
            result = Array.Empty<Rating>();

            var tickers = entities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            var count = context.Tickers.Where(x => tickers.Contains(x.Name)).Count();

            var isValid = tickers.Length == count;

            if (isValid)
                result = entities.ToArray();

            return isValid;
        }
        public Rating GetIntersectedContextEntity(Rating entity) => context.Ratings.Find(entity.Place);
        public IQueryable<Rating> GetIntersectedContextEntities(IEnumerable<Rating> entities)
        {
            var places = entities.Select(y => y.Place).ToArray();
            return context.Ratings.Where(x => places.Contains(x.Place));
        }
        public bool UpdateEntity(Rating oldResult, Rating newResult)
        {
            var isCompare = oldResult.Place == newResult.Place;
            
            if (isCompare)
            {
                oldResult.Place = newResult.Place;
                oldResult.Result = newResult.Result;
                oldResult.PriceComparison = newResult.PriceComparison;
                oldResult.ReportComparison = newResult.ReportComparison;
                oldResult.UpdateTime = DateTime.UtcNow;
            }

            return isCompare;
        }
    }
}
