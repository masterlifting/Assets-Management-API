using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class CoefficientRepositoryHandler : RepositoryHandler<Coefficient, DatabaseContext>
{
    private readonly DatabaseContext context;
    public CoefficientRepositoryHandler(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Coefficient>> RunUpdateRangeHandlerAsync(IEnumerable<Coefficient> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.SourceId, x.Year, x.Quarter),
                y => (y.CompanyId, y.SourceId, y.Year, y.Quarter),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Pe = New.Pe;
            Old.Pb = New.Pb;
            Old.DebtLoad = New.DebtLoad;
            Old.Profitability = New.Profitability;
            Old.Roa = New.Roa;
            Old.Roe = New.Roe;
            Old.Eps = New.Eps;

            Old.StatusId = New.StatusId;
            Old.Result = New.Result;
        }

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<Coefficient>> RunDeleteRangeHandlerAsync(IEnumerable<Coefficient> entities)
    {
        var comparer = new DataQuarterComparer<Coefficient>();
        var result = new List<Coefficient>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Coefficients.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public override IQueryable<Coefficient> GetExist(IEnumerable<Coefficient> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var sourceIds = entities
            .GroupBy(x => x.SourceId)
            .Select(x => x.Key);
        var years = entities
            .GroupBy(x => x.Year)
            .Select(x => x.Key);
        var quarters = entities
            .GroupBy(x => x.Quarter)
            .Select(x => x.Key);

        return context.Coefficients
            .Where(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId)
                && years.Contains(x.Year)
                && quarters.Contains(x.Quarter));
    }
}