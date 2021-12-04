using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Comparators;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class CompanyRepository : IRepositoryHandler<Entities.Company>
{
    private readonly DatabaseContext context;
    public CompanyRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref Entities.Company entity) => Task.CompletedTask;

    public Task GetCreateHandlerAsync(ref Entities.Company[] entities)
    {
        var exist = GetExist(entities);
        
        var comparer = new CompanyComparer<Entities.Company>();
        entities = entities.Distinct(comparer).ToArray();

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
        
    public Task GetUpdateHandlerAsync(ref Entities.Company entity)
    {
        var ctxEntity = context.Companies.FindAsync(entity.Id).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Entities.Company)} data not found. ");

        ctxEntity.Name = entity.Name;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Entities.Company[] entities)
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

    public async Task<IList<Entities.Company>> GetDeleteHandlerAsync(IReadOnlyCollection<Entities.Company> entities)
    {
        var comparer = new CompanyComparer<Entities.Company>();
        var ctxEntities = await context.Companies.ToArrayAsync();
        return ctxEntities.Except(entities, comparer).ToArray();
    }

    public Task SetPostProcessAsync(Entities.Company entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Entities.Company> entities) => Task.CompletedTask;

    private IQueryable<Entities.Company> GetExist(IEnumerable<Entities.Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant());

        return context.Companies.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }
}