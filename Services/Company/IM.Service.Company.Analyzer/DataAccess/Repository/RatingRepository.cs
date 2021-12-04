using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Analyzer.DataAccess.Comparators;
using IM.Service.Company.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.DataAccess.Repository;

public class RatingRepository : IRepositoryHandler<Rating>
{
    private readonly DatabaseContext context;
    public RatingRepository(DatabaseContext context) => this.context = context;

    public Task GetCreateHandlerAsync(ref Rating entity)
    {
        var ratings = context.Ratings.ToListAsync().GetAwaiter().GetResult();

        ratings.Add(entity);

        SetPlaces(ref ratings);

        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Rating[] entities)
    {
        var comparer = new RatingComparer();
        entities = entities.Distinct(comparer).ToArray();

        var exist = GetExist(entities).ToListAsync().GetAwaiter().GetResult();
        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        if (!entities.Any())
            return Task.CompletedTask;

        var ratings = context.Ratings.ToListAsync().GetAwaiter().GetResult();
        ratings = ratings.Except(entities, comparer).ToList();
        ratings = ratings.Concat(entities).ToList();

        SetPlaces(ref ratings);

        entities = ratings.Join(entities, x => x.CompanyId, y => y.CompanyId, (x, _) => x).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref Rating entity)
    {
        var ratings = context.Ratings.ToListAsync().GetAwaiter().GetResult();

        var companyId = entity.CompanyId;
        var ctxEntity = ratings.Find(x => x.CompanyId == companyId);

        if (ctxEntity is null)
            throw new DataException($"{nameof(Rating)} not found");

        ratings.Remove(ctxEntity);

        var newRating = new Rating
        {
            Id = ctxEntity.Id,
            CompanyId = companyId,
            Result = entity.Result
        };

        ratings.Add(newRating);

        SetPlaces(ref ratings);

        entity = newRating;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Rating[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        if (!exist.Any())
        {
            entities = Array.Empty<Rating>();
            return Task.CompletedTask;
        }

        var result = exist.Join(entities, x => x.CompanyId, y => y.CompanyId, (x, y) => (Old: x, New: y)).ToArray();
        foreach (var (Old, New) in result)
        {
            Old.Result = New.Result;
            Old.UpdateTime = DateTime.UtcNow;
        }

        entities = result.Select(x => x.Old).ToArray();

        var comparer = new RatingComparer();

        var ratings = context.Ratings.ToListAsync().GetAwaiter().GetResult();
        ratings = ratings.Except(entities, comparer).ToList();
        ratings = ratings.Concat(entities).ToList();

        SetPlaces(ref ratings);

        entities = ratings.Join(entities, x => x.CompanyId, y => y.CompanyId, (x, _) => x).ToArray();

        return Task.CompletedTask;
    }

    public async Task<IList<Rating>> GetDeleteHandlerAsync(IReadOnlyCollection<Rating> entities)
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

    public Task SetPostProcessAsync(Rating entity) => Task.CompletedTask;
    public Task SetPostProcessAsync(IReadOnlyCollection<Rating> entities) => Task.CompletedTask;

    private IQueryable<Rating> GetExist(IEnumerable<Rating> entities)
    {
        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.Ratings.Where(x => existData.Contains(x.CompanyId));
    }

    private static void SetPlaces(ref List<Rating> ratings)
    {
        ratings.Sort((x, y) => x.Result < y.Result ? 1 : x.Result > y.Result ? -1 : 0);

        for (var i = 0; i < ratings.Count; i++)
            ratings[i].Place = i + 1;
    }
}