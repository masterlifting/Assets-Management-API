using System.Collections.Generic;
using System.Linq;
using CommonServices.RepositoryService;
using IM.Services.Company.Reports.DataAccess.Entities;

namespace IM.Services.Company.Reports.DataAccess.Repository
{
    public class ReportRepository : IRepository<Report>
    {
        private readonly ReportsContext context;
        public ReportRepository(ReportsContext context) => this.context = context;

        public bool TryCheckEntity(Report entity, out Report result)
        {
            result = entity;
            return entity is not null && context.Tickers.Any(x => x.Name.Equals(entity.TickerName));
        }
        public bool TryCheckEntities(IEnumerable<Report> entities, out Report[] result)
        {
            result = entities.ToArray();

            var tickers = result.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            var count = context.Tickers.Count(x => tickers.Contains(x.Name));

            return tickers.Length == count;
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
                oldResult.Turnover = newResult.Turnover;
                oldResult.LongTermDebt = newResult.LongTermDebt;
                oldResult.Asset = newResult.Asset;
                oldResult.CashFlow = newResult.CashFlow;
                oldResult.Obligation = newResult.Obligation;
                oldResult.ProfitGross = newResult.ProfitGross;
                oldResult.ProfitNet = newResult.ProfitNet;
                oldResult.Revenue = newResult.Revenue;
                oldResult.ShareCapital = newResult.ShareCapital;
                oldResult.Dividend = newResult.Dividend;
            }

            return isCompare;
        }
    }
}
