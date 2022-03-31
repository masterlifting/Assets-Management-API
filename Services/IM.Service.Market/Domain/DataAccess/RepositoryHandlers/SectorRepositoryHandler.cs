using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class SectorRepositoryHandler : RepositoryHandler<Sector, DatabaseContext>
{
    private readonly DatabaseContext context;
    public SectorRepositoryHandler(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Sector>> GetUpdateRangeHandlerAsync(IEnumerable<Sector> entities)
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
        }

        return result.Select(x => x.Old).ToArray();
    }
    public override async Task<IEnumerable<Sector>> GetDeleteRangeHandlerAsync(IEnumerable<Sector> entities)
    {
        var comparer = new SectorComparer();
        var result = new List<Sector>();

        foreach (var group in entities.GroupBy(x => x.Name))
        {
            var dbEntities = await context.Sectors.Where(x => x.Name.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }
    public override IQueryable<Sector> GetExist(IEnumerable<Sector> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key)
            .ToArray();

        return context.Sectors.Where(x => existData.Contains(x.Name));
    }
}