using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class AnalyzedEntityRepository : IRepositoryHandler<AnalyzedEntity>
{
    private readonly DatabaseContext context;
    public AnalyzedEntityRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref AnalyzedEntity entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref AnalyzedEntity[] entities)
    {
        var exist = GetExist(entities);
        var comparer = new AnalyzedEntityComparer();
        entities = entities.Distinct(comparer).ToArray();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref AnalyzedEntity entity)
    {
        var ctxEntity = context.AnalyzedEntities.FindAsync(entity.CompanyId, entity.AnalyzedEntityTypeId, entity.Date).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(AnalyzedEntity)} data not found. ");

        ctxEntity.Result = entity.Result;
        ctxEntity.StatusId = entity.StatusId;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref AnalyzedEntity[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => ( x.CompanyId, x.AnalyzedEntityTypeId, x.Date), y => (y.CompanyId, y.AnalyzedEntityTypeId, y.Date),
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

    public async Task<IList<AnalyzedEntity>> GetDeleteHandlerAsync(IReadOnlyCollection<AnalyzedEntity> entities)
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

    public Task SetPostProcessAsync(AnalyzedEntity entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<AnalyzedEntity> entities) => Task.CompletedTask;

    private IQueryable<AnalyzedEntity> GetExist(AnalyzedEntity[] entities)
    {
        var dateMin = entities.Min(x => x.Date);
        var dateMax = entities.Max(x => x.Date);

        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.AnalyzedEntities.Where(x => existData.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
    }
}