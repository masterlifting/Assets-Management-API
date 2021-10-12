using CommonServices.RepositoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Company.Analyzer.DataAccess.Repository
{
    public class RatingRepository : IRepositoryHandler<Rating>
    {
        private readonly DatabaseContext context;
        public RatingRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, Rating? checkedEntity)> TryCheckEntityAsync(Rating entity) =>
            (await context.Tickers.AnyAsync(x => x.Name.Equals(entity.TickerName)), entity);
        public async Task<(bool isSuccess, Rating[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<Rating> entities)
        {
            var arrayEntities = entities.ToArray();

            var tickers = arrayEntities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            var count = await context.Tickers.CountAsync(x => tickers.Contains(x.Name));

            return (tickers.Length == count, arrayEntities);
        }
        public async Task<Rating?> GetAlreadyEntityAsync(Rating entity) => await context.Ratings.FindAsync(entity.Place);

        public IQueryable<Rating> GetAlreadyEntitiesQuery(IEnumerable<Rating> entities)
        {
            var places = entities.Select(y => y.Place).ToArray();
            return context.Ratings.Where(x => places.Contains(x.Place));
        }
        public bool IsUpdate(Rating contextEntity, Rating newEntity)
        {
            var isCompare = contextEntity.Place == newEntity.Place;

            if (isCompare)
            {
                contextEntity.Place = newEntity.Place;
                contextEntity.Result = newEntity.Result;
                contextEntity.PriceComparison = newEntity.PriceComparison;
                contextEntity.ReportComparison = newEntity.ReportComparison;
                contextEntity.UpdateTime = DateTime.UtcNow;
            }

            return isCompare;
        }
    }
}
