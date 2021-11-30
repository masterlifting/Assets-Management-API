using IM.Service.Common.Net.RepositoryService;

using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Analyzer.DataAccess.Comparators;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class RatingDataRepository : IRepositoryHandler<RatingData>
{
    private readonly DatabaseContext context;
    public RatingDataRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref RatingData entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref RatingData[] entities)
    {
        var exist = GetExist(entities);
        
        var comparer = new RatingDataComparer();
        entities = entities.Distinct(comparer).ToArray();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
    
    public Task GetUpdateHandlerAsync(ref RatingData entity)
    {
        var ctxEntity = context.RatingData.FindAsync(entity.Id).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(RatingData)} data not found. ");

        ctxEntity.Result = entity.Result;
        ctxEntity.AnalyzedEntityTypeId = entity.AnalyzedEntityTypeId;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref RatingData[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => x.Id, y => y.Id,
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Result = New.Result;
            Old.AnalyzedEntityTypeId = New.AnalyzedEntityTypeId;
        }

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public async Task<IList<RatingData>> GetDeleteHandlerAsync(IReadOnlyCollection<RatingData> entities)
    {
        var comparer = new RatingDataComparer();
        var result = new List<RatingData>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.RatingData.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(RatingData entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<RatingData> entities) => Task.CompletedTask;

    private IQueryable<RatingData> GetExist(IEnumerable<RatingData> entities)
    {
        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.RatingData.Where(x => existData.Contains(x.CompanyId));
    }
}