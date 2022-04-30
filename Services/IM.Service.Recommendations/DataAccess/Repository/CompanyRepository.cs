using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Recommendations.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Recommendations.DataAccess.Repository;

public class CompanyRepository : RepositoryHandler<Company, DatabaseContext>
{
    private readonly DatabaseContext context;
    public CompanyRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Company>> RunUpdateRangeHandlerAsync(IEnumerable<Company> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.Id, y => y.Id, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Company> GetExist(IEnumerable<Company> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);

        return context.Companies.Where(x => ids.Contains(x.Id));
    }
}