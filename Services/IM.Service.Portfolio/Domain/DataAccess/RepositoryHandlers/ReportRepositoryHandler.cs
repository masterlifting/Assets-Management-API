using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;

public class ReportRepositoryHandler : RepositoryHandler<Report>
{
    private readonly DatabaseContext context;
    public ReportRepositoryHandler(DatabaseContext context) => this.context = context;

    public override async Task<IEnumerable<Report>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Report> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.AccountUserId, x.AccountBrokerId, x.Name),
                y => (y.AccountUserId, y.AccountBrokerId, y.Name),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.DateStart = New.DateStart;
            Old.DateEnd = New.DateEnd;
            Old.ContentType = New.ContentType;
            Old.Payload = New.Payload;
            Old.AccountName = New.AccountName;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Report> GetExist(IEnumerable<Report> entities)
    {
        entities = entities.ToArray();

        var userIds = entities
            .GroupBy(x => x.AccountUserId)
            .Select(x => x.Key);
        var brockerIds = entities
            .GroupBy(x => x.AccountBrokerId)
            .Select(x => x.Key);
        var names = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key);

        return context.Reports
            .Where(x =>
                userIds.Contains(x.AccountUserId)
                && brockerIds.Contains(x.AccountBrokerId)
                && names.Contains(x.Name));
    }
}