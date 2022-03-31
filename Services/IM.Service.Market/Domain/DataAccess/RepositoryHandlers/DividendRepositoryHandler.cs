using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class DividendRepositoryHandler : RepositoryHandler<Dividend, DatabaseContext>
{
    private readonly DatabaseContext context;
    public DividendRepositoryHandler(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Dividend>> GetUpdateRangeHandlerAsync(IEnumerable<Dividend> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.SourceId, x.Date),
                y => (y.CompanyId, y.SourceId, y.Date),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.CurrencyId = New.CurrencyId;

            Old.Value = New.Value;

            Old.StatusId = New.StatusId;
            Old.Result = New.Result;
        }

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<Dividend>> GetDeleteRangeHandlerAsync(IEnumerable<Dividend> entities)
    {
        var comparer = new DataDateComparer<Dividend>();
        var result = new List<Dividend>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Dividends.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public override IQueryable<Dividend> GetExist(IEnumerable<Dividend> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var sourceIds = entities
            .GroupBy(x => x.SourceId)
            .Select(x => x.Key);
        var dates = entities
            .GroupBy(x => x.Date)
            .Select(x => x.Key);

        return context.Dividends.Where(x => companyIds.Contains(x.CompanyId) && sourceIds.Contains(x.SourceId) && dates.Contains(x.Date));
    }
}