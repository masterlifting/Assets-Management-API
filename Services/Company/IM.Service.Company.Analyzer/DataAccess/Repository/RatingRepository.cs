using CommonServices.RepositoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using IM.Service.Company.Analyzer.DataAccess.Entities;

namespace IM.Service.Company.Analyzer.DataAccess.Repository
{
    public class RatingRepository : IRepository<Rating>
    {
        private readonly DatabaseContext context;
        public RatingRepository(DatabaseContext context) => this.context = context;

        public bool TryCheckEntity(Rating entity, out Rating? result)
        {
            result = entity;
            return context.Tickers.Any(x => x.Name.Equals(entity.TickerName));
        }
        public bool TryCheckEntities(IEnumerable<Rating> entities, out Rating[] result)
        {
            result = entities.ToArray();

            var tickers = result.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            var count = context.Tickers.Count(x => tickers.Contains(x.Name));

            return tickers.Length == count;
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
