using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.Entities.Catalogs;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class IndustryRepositoryHandler : RepositoryHandler<Industry>
{
    private readonly DatabaseContext context;
    public IndustryRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Industry>> RunUpdateRangeHandlerAsync(IEnumerable<Industry> entities)
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
    public override IQueryable<Industry> GetExist(IEnumerable<Industry> entities)
    {
        var existData = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key)
            .ToArray();

        return context.Industries.Where(x => existData.Contains(x.Name));
    }
}