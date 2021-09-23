using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CommonServices.RepositoryService;

using IM.Service.Company.Reports.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Company.Reports.DataAccess.Repository
{
    public class ReportRepository : IRepository<Report>
    {
        private readonly DatabaseContext context;
        public ReportRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, Report? checkedEntity)> TryCheckEntityAsync(Report entity)
        {
            var isSuccess = await context.Tickers.AnyAsync(x => x.Name.Equals(entity.TickerName));

            if (!isSuccess)
                return (false, null);

            var sourceType = entity.SourceType.ToLowerInvariant().Trim();
            var isSourceType = await context.SourceTypes.AnyAsync(x => entity.SourceType.ToLowerInvariant().Equals(sourceType));

            if (!isSourceType)
                entity.SourceType = nameof(Enums.ReportSourceTypes.Default);

            return (true, entity);
        }

        public async Task<(bool isSuccess, Report[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<Report> entities)
        {
            var result = entities.ToArray();

            var tickers = result.GroupBy(y => y.TickerName).Select(y => y.Key).ToArray();
            var count = await context.Tickers.CountAsync(x => tickers.Contains(x.Name));

            if (tickers.Length != count)
                return (false, Array.Empty<Report>());

            var sourceTypes = await context.SourceTypes.Select(x => x.Name).ToArrayAsync();
            foreach (var entity in result)
                if (!sourceTypes.Any(x => x.Equals(entity.SourceType, StringComparison.OrdinalIgnoreCase)))
                    entity.SourceType = nameof(Enums.ReportSourceTypes.Default);

            return (true, result);
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

            // ReSharper disable once InvertIf
            if (isCompare)
            {
                contextEntity.SourceType = newEntity.SourceType;
                contextEntity.Multiplier = newEntity.Multiplier;
                contextEntity.StockVolume = newEntity.StockVolume;
                contextEntity.Turnover = newEntity.Turnover;
                contextEntity.LongTermDebt = newEntity.LongTermDebt;
                contextEntity.Asset = newEntity.Asset;
                contextEntity.CashFlow = newEntity.CashFlow;
                contextEntity.Obligation = newEntity.Obligation;
                contextEntity.ProfitGross = newEntity.ProfitGross;
                contextEntity.ProfitNet = newEntity.ProfitNet;
                contextEntity.Revenue = newEntity.Revenue;
                contextEntity.ShareCapital = newEntity.ShareCapital;
                contextEntity.Dividend = newEntity.Dividend;
            }

            return isCompare;
        }
    }
}
