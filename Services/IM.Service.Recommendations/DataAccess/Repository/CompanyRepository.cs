using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Recommendations.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.DataAccess.Repository;

public class CompanyRepository : IRepositoryHandler<Company>
{
    private readonly DatabaseContext context;
    public CompanyRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref Company entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Company[] entities)
    {
        var exist = GetExist(entities);
        var comparer = new CompanyComparer<Company>();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
        
    public Task GetUpdateHandlerAsync(ref Company entity)
    {
        var ctxEntity = context.Companies.FindAsync(entity.Id).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Company)} data not found. ");

        ctxEntity.Name = entity.Name;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Company[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => x.Id, y => y.Id,
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Name = New.Name;

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public async Task<IList<Company>> GetDeleteHandlerAsync(IReadOnlyCollection<Company> entities)
    {
        var comparer = new CompanyComparer<Company>();
        var companies = await context.Companies.ToArrayAsync();
        return companies.Except(entities, comparer).ToArray();
    }

    public Task SetPostProcessAsync(Company entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Company> entities) => Task.CompletedTask;

    private IQueryable<Company> GetExist(IEnumerable<Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant())
            .ToArray();

        return context.Companies.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }
}