using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class IndustryRepositoryHandler : RepositoryHandler<Industry, DatabaseContext>
{
    private readonly DatabaseContext context;
    public IndustryRepositoryHandler(DatabaseContext context) : base(context) => this.context = context;

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
    public override async Task<IEnumerable<Industry>> RunDeleteRangeHandlerAsync(IEnumerable<Industry> entities)
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
            .Select(x => x.Key)
            .ToArray();

        return context.Industries.Where(x => existData.Contains(x.Name));
    }
}