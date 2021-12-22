using IM.Service.Common.Net.RepositoryService;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Comparators;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class CompanyRepository : RepositoryHandler<Entities.Company, DatabaseContext>
{
    private readonly DatabaseContext context;
    public CompanyRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Entities.Company>> GetUpdateRangeHandlerAsync(IEnumerable<Entities.Company> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => x.Id,
                y => y.Id,
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Name = New.Name;

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<Entities.Company>> GetDeleteRangeHandlerAsync(IEnumerable<Entities.Company> entities)
    {
        var comparer = new CompanyComparer<Entities.Company>();
        var ctxEntities = await context.Companies.ToArrayAsync();
        return ctxEntities.Except(entities, comparer);
    }
    public override IQueryable<Entities.Company> GetExist(IEnumerable<Entities.Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant());

        return context.Companies.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }
}