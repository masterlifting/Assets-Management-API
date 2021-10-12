using CommonServices.RepositoryService;

using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.DataAccess.Repository
{
    public class ReportRepository : IRepositoryHandler<Report>
    {
        private readonly DatabaseContext context;
        public ReportRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, Report? checkedEntity)> TryCheckEntityAsync(Report entity) =>
            (await context.Tickers.AnyAsync(x => x.Name.Equals(entity.TickerName)), entity);

        public async Task<(bool isSuccess, Report[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<Report> entities)
        {
            var result = entities.ToArray();

            var tickers = result.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            var count = await context.Tickers.CountAsync(x => tickers.Contains(x.Name));

            return (tickers.Length == count, result);
        }
        public async Task<Report?> GetAlreadyEntityAsync(Report entity) => await context.Reports.FindAsync(entity.TickerName, entity.Year, entity.Quarter);
        public IQueryable<Report> GetAlreadyEntitiesQuery(IEnumerable<Report> entities)
        {
            var tickers = entities.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            return context.Reports.Where(x => tickers.Contains(x.TickerName));
        }
        public bool IsUpdate(Report contextEntity, Report newEntity)
        {
            var isCompare = (contextEntity.TickerName, contextEntity.Year, contextEntity.Quarter) == (newEntity.TickerName, newEntity.Year, newEntity.Quarter);

            if (isCompare)
            {
                contextEntity.Result = newEntity.Result;
                contextEntity.StatusId = newEntity.StatusId;
            }

            return isCompare;
        }
    }
}
