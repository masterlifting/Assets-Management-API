using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Portfolio.DataAccess.Repositories;

public class ReportRepository : RepositoryHandler<Report, DatabaseContext>
{
    private readonly DatabaseContext context;
    public ReportRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Report>> RunUpdateRangeHandlerAsync(IEnumerable<Report> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.UserId, x.BrokerId, x.Name),
                y => (y.UserId, y.BrokerId, y.Name),
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
            .GroupBy(x => x.UserId)
            .Select(x => x.Key);
        var brockerIds = entities
            .GroupBy(x => x.BrokerId)
            .Select(x => x.Key);
        var names = entities
            .GroupBy(x => x.Name)
            .Select(x => x.Key);

        return context.Reports
            .Where(x =>
                userIds.Contains(x.UserId)
                && brockerIds.Contains(x.BrokerId)
                && names.Contains(x.Name));
    }
}