using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.DataAccess.Repository;

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
        {
            Old.Name = New.Name;
            Old.IndustryId = New.IndustryId;
            Old.Description = New.Description;
        }

        return result.Select(x => x.Old).ToArray();
    }
    public override async Task<IEnumerable<Entities.Company>> GetDeleteRangeHandlerAsync(IEnumerable<Entities.Company> entities)
    {
        var comparer = new CompanyComparer<Entities.Company>();
        var companies = await context.Companies.ToArrayAsync();
        return companies.Except(entities, comparer).ToArray();
    }

    public override IQueryable<Entities.Company> GetExist(IEnumerable<Entities.Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key)
            .ToArray();

        return context.Companies.Where(x => existData.Contains(x.Id));
    }
}