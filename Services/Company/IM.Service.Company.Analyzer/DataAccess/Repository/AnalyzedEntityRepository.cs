using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class AnalyzedEntityRepository : RepositoryHandler<AnalyzedEntity, DatabaseContext>
{
    private readonly DatabaseContext context;
    public AnalyzedEntityRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<AnalyzedEntity>> GetUpdateRangeHandlerAsync(IEnumerable<AnalyzedEntity> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.AnalyzedEntityTypeId, x.Date),
                y => (y.CompanyId, y.AnalyzedEntityTypeId, y.Date),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Result = New.Result;
            Old.StatusId = New.StatusId;
        }

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<AnalyzedEntity>> GetDeleteRangeHandlerAsync(IEnumerable<AnalyzedEntity> entities)
    {
        var comparer = new AnalyzedEntityComparer();
        var result = new List<AnalyzedEntity>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.AnalyzedEntities.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }
    public override IQueryable<AnalyzedEntity> GetExist(IEnumerable<AnalyzedEntity> entities)
    {
        entities = entities.ToArray();

        var dateMin = entities.Min(x => x.Date);
        var dateMax = entities.Max(x => x.Date);

        var existData = entities
            .GroupBy(x => (x.CompanyId,x.AnalyzedEntityTypeId))
            .Select(x => x.Key)
            .ToArray();

        var companyIds = existData.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var typeIds = existData.GroupBy(x => x.AnalyzedEntityTypeId).Select(x => x.Key).ToArray();

        var dbc = context.AnalyzedEntities.Where(x => companyIds.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
        var dbt = context.AnalyzedEntities.Where(x => typeIds.Contains(x.AnalyzedEntityTypeId) && x.Date >= dateMin && x.Date <= dateMax);

        return dbc.Join(dbt,
            x => new {x.CompanyId, x.AnalyzedEntityTypeId},
            y => new { y.CompanyId, y.AnalyzedEntityTypeId},
            (x, _) => x);
    }
}