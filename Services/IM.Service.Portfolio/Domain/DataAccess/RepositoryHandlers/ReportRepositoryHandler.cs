using System;
using IM.Service.Shared.SqlAccess;
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
                x => (x.Id, x.ProviderId, x.AccountId),
                y => (y.Id, y.ProviderId, y.AccountId),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.UpdateTime = DateTime.UtcNow;
            Old.DateStart = New.DateStart;
            Old.DateEnd = New.DateEnd;
            Old.ContentType = New.ContentType;
            Old.Payload = New.Payload;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Report> GetExist(IEnumerable<Report> entities)
    {
        entities = entities.ToArray();

        var ids = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key);
        var providerIds = entities
            .GroupBy(x => x.ProviderId)
            .Select(x => x.Key);
        var accountIds = entities
            .GroupBy(x => x.AccountId)
            .Select(x => x.Key);

        return context.Reports.Where(x => ids.Contains(x.Id) && providerIds.Contains(x.ProviderId) && accountIds.Contains(x.AccountId));
    }
}