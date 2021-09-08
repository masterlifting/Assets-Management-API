using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess.Entities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class ReportRepository : IRepository<Report>
    {
        private readonly AnalyzerContext context;
        public ReportRepository(AnalyzerContext context) => this.context = context;

        public bool TryCheckEntity(Report entity, out Report? result)
        {
            result = null;
            var isValid = context.Tickers.Where(x => x.Name.Equals(entity.TickerName)).Any();

            if (isValid)
                result = entity;

            return isValid;
        }
        public bool TryCheckEntities(IEnumerable<Report> entities, out Report[] result)
        {
            result = Array.Empty<Report>();

            var tickers = entities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            var count = context.Tickers.Where(x => tickers.Contains(x.Name)).Count();

            var isValid = tickers.Length == count;

            if (isValid)
                result = entities.ToArray();

            return isValid;
        }
        public Report GetIntersectedContextEntity(Report entity) => context.Reports.Find(entity.TickerName, entity.Year, entity.Quarter);
        public IQueryable<Report> GetIntersectedContextEntities(IEnumerable<Report> entities)
        {
            var tickers = entities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            return context.Reports.Where(x => tickers.Contains(x.TickerName));
        }
        public bool UpdateEntity(Report oldResult, Report newResult)
        {
            var isCompare = (oldResult.TickerName, oldResult.Year, oldResult.Quarter) == (newResult.TickerName, newResult.Year, newResult.Quarter);

            if (isCompare)
            {
                oldResult.Result = newResult.Result;
                oldResult.StatusId = newResult.StatusId;
                oldResult.Date = DateTime.UtcNow;
            }

            return isCompare;
        }
    }
}
