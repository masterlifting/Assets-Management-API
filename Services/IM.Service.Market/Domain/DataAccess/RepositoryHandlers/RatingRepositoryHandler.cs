using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class RatingRepositoryHandler : RepositoryHandler<Rating>
{
    private readonly DatabaseContext context;
    public RatingRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Rating>> RunUpdateRangeHandlerAsync(IEnumerable<Rating> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.CompanyId, y => y.CompanyId, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.ResultCoefficient = New.ResultCoefficient;
            Old.ResultDividend = New.ResultDividend;
            Old.ResultPrice = New.ResultPrice;
            Old.ResultReport = New.ResultReport;

            Old.Result = New.Result;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Rating> GetExist(IEnumerable<Rating> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);

        return context.Rating.Where(x => companyIds.Contains(x.CompanyId));
    }
}