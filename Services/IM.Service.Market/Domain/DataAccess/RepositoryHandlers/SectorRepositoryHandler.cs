using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.Entities.Catalogs;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class SectorRepositoryHandler : RepositoryHandler<Sector>
{
    private readonly DatabaseContext context;
    public SectorRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Sector>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Sector> entities)
    {
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
    public override IQueryable<Sector> GetExist(IEnumerable<Sector> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key)
            .ToArray();

        return context.Sectors.Where(x => existData.Contains(x.Name));
    }
}