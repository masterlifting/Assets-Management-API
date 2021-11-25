using System.Collections.Generic;
using IM.Service.Common.Net.RepositoryService;

using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Comparators;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class ReportRepository : IRepositoryHandler<Report>
{
    private readonly DatabaseContext context;
    public ReportRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref Report entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Report[] entities)
    {
        var exist = GetExist(entities);
        
        var comparer = new CompanyQuarterComparer<Report>();
        entities = entities.Distinct(comparer).ToArray();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref Report entity)
    {
        var ctxEntity = context.Reports.FindAsync(entity.CompanyId, entity.Year, entity.Quarter).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Report)} data not found. ");

        ctxEntity.Result = entity.Result;
        ctxEntity.StatusId = entity.StatusId;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Report[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => (x.CompanyId, x.Year, x.Quarter), y => (y.CompanyId, y.Year, y.Quarter),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Result = New.Result;
            Old.StatusId = New.StatusId;
        }

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public async Task<IList<Report>> GetDeleteHandlerAsync(IReadOnlyCollection<Report> entities)
    {
        var comparer = new CompanyQuarterComparer<Report>();
        var result = new List<Report>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Reports.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(Report entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Report> entities) => Task.CompletedTask;

    private IQueryable<Report> GetExist(Report[] entities)
    {
        var yearMin = entities.Min(x => x.Year);
        var yearMax = entities.Max(x => x.Year);

        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.Reports.Where(x => existData.Contains(x.CompanyId) && x.Year >= yearMin && x.Year <= yearMax);
    }
}