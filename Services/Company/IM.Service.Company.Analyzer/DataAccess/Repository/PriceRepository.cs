using System.Collections.Generic;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class PriceRepository : IRepositoryHandler<Price>
{
    private readonly DatabaseContext context;
    public PriceRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref Price entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Price[] entities)
    {
        var exist = GetExist(entities);
        var comparer = new CompanyDateComparer<Price>();
        entities = entities.Distinct(comparer).ToArray();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref Price entity)
    {
        var ctxEntity = context.Prices.FindAsync(entity.CompanyId, entity.Date).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Price)} data not found. ");

        ctxEntity.Result = entity.Result;
        ctxEntity.StatusId = entity.StatusId;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Price[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => (x.CompanyId, x.Date), y => (y.CompanyId, y.Date),
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

    public async Task<IList<Price>> GetDeleteHandlerAsync(IReadOnlyCollection<Price> entities)
    {
        var comparer = new CompanyDateComparer<Price>();
        var result = new List<Price>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Prices.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(Price entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Price> entities) => Task.CompletedTask;

    private IQueryable<Price> GetExist(Price[] entities)
    {
        var dateMin = entities.Min(x => x.Date);
        var dateMax = entities.Max(x => x.Date);

        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.Prices.Where(x => existData.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
    }
}