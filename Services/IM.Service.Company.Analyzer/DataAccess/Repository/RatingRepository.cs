using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class RatingRepository : RepositoryHandler<Rating, DatabaseContext>
{
    private readonly DatabaseContext context;
    public RatingRepository(DatabaseContext context) : base(context) => this.context = context;

    public override async Task<IEnumerable<Rating>> GetUpdateRangeHandlerAsync(IEnumerable<Rating> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, 
                x => x.CompanyId, 
                y => y.CompanyId, 
                (x, y) => (Old: x, New: y))
            .ToArray();
        
        foreach (var (Old, New) in result)
        {
            Old.Result = New.Result;
            Old.ResultPrice = New.ResultPrice;
            Old.ResultCoefficient = New.ResultCoefficient;
            Old.ResultReport = New.ResultReport;
            Old.Date = DateOnly.FromDateTime(DateTime.UtcNow);
            Old.Time = TimeOnly.FromDateTime(DateTime.UtcNow);
        }

        return result.Select(x => x.Old).ToArray();
    }
    public override async Task<IEnumerable<Rating>> GetDeleteRangeHandlerAsync(IEnumerable<Rating> entities)
    {
        var comparer = new RatingComparer();
        var result = new List<Rating>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Ratings.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }
    public override IQueryable<Rating> GetExist(IEnumerable<Rating> entities)
    {
        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.Ratings.Where(x => existData.Contains(x.CompanyId));
    }
}