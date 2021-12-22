using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.DataAccess.Comparators;
using IM.Service.Company.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.DataAccess.Repository;

public class IndustryRepository : RepositoryHandler<Industry, DatabaseContext>
{
    private readonly DatabaseContext context;
    public IndustryRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Industry>> GetUpdateRangeHandlerAsync(IEnumerable<Industry> entities)
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
            Old.Description = New.Description;
            Old.SectorId = New.SectorId;
        }

        return result.Select(x => x.Old).ToArray();
    }
    public override async Task<IEnumerable<Industry>> GetDeleteRangeHandlerAsync(IEnumerable<Industry> entities)
    {
        var comparer = new IndustryComparer();
        var result = new List<Industry>();

        foreach (var group in entities.GroupBy(x => x.SectorId))
        {
            var dbEntities = await context.Industries.Where(x => x.SectorId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }
    public override IQueryable<Industry> GetExist(IEnumerable<Industry> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key.ToLowerInvariant())
            .ToArray();

        return context.Industries.Where(x => existData.Contains(x.Name.ToLowerInvariant()));
    }
}