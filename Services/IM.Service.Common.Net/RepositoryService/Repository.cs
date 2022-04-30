using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helpers.LogHelper;
using static IM.Service.Common.Net.Helpers.ServiceHelper;

namespace IM.Service.Common.Net.RepositoryService;

public class Repository<TEntity, TContext> where TEntity : class where TContext : DbContext
{
    private const string OK = "OK";

    private readonly TContext context;
    private readonly IRepositoryHandler<TEntity> handler;

    private readonly ILogger<TEntity> logger;

    protected Repository(
        ILogger<TEntity> logger
        , TContext context
        , IRepositoryHandler<TEntity> handler)
    {
        this.logger = logger;
        this.context = context;
        this.handler = handler;
    }

    public async Task<TEntity> CreateAsync(TEntity entity, string info)
    {
        entity = await handler.RunCreateHandlerAsync(entity).ConfigureAwait(false);
        await context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
        await handler.RunPostProcessAsync(RepositoryActions.Create, entity).ConfigureAwait(false);

        logger.LogDebug(nameof(CreateRangeAsync), info, OK);

        return entity;
    }
    public async Task<TEntity[]> CreateRangeAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
            return Array.Empty<TEntity>();

        entities = await handler.RunCreateRangeHandlerAsync(entities, comparer).ConfigureAwait(false);

        var result = entities.ToArray();
        var count = result.Length;

        if (result.Any())
        {
            await context.Set<TEntity>().AddRangeAsync(result).ConfigureAwait(false);
            count = await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Create, result).ConfigureAwait(false);
        }

        logger.LogDebug(nameof(CreateRangeAsync), info, count);

        return result;
    }

    public async Task<TEntity> UpdateAsync(object[] id, TEntity entity, string info)
    {
        entity = await handler.RunUpdateHandlerAsync(id, entity).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
        await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);

        logger.LogDebug(nameof(UpdateRangeAsync), info, OK);

        return entity;
    }
    public async Task<TEntity> UpdateAsync(TEntity entity, string info)
    {
        entity = await handler.RunUpdateHandlerAsync(entity).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
        await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);

        logger.LogDebug(nameof(UpdateRangeAsync), info, OK);

        return entity;
    }
    public async Task<TEntity[]> UpdateRangeAsync(IEnumerable<TEntity> entities, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
            return Array.Empty<TEntity>();

        entities = await handler.RunUpdateRangeHandlerAsync(entities).ConfigureAwait(false);

        var result = entities.ToArray();
        var count = result.Length;

        if (result.Any())
        {
            context.Set<TEntity>().UpdateRange(result);
            count = await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Update, result).ConfigureAwait(false);
        }

        logger.LogDebug(nameof(UpdateRangeAsync), info, count);

        return result;
    }

    public async Task<TEntity> CreateUpdateAsync(object[] id, TEntity entity, string info)
    {
        try
        {
            entity = await handler.RunCreateHandlerAsync(entity).ConfigureAwait(false);
            await context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Create, entity).ConfigureAwait(false);

            logger.LogDebug(nameof(CreateUpdateRangeAsync), info, "Created: " + OK);
        }
        catch
        {
            entity = await handler.RunUpdateHandlerAsync(id, entity).ConfigureAwait(false);
            context.Entry(entity).State = EntityState.Modified;
            context.Set<TEntity>().Update(entity);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);

            logger.LogDebug(nameof(CreateUpdateRangeAsync), info, "Updated: " + OK);
        }
        return entity;
    }
    public async Task<TEntity[]> CreateUpdateRangeAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
            return Array.Empty<TEntity>();

        var createEntities = await handler.RunCreateRangeHandlerAsync(entities, comparer).ConfigureAwait(false);
        var createResult = createEntities.ToArray();

        if (createResult.Any())
            await context.Set<TEntity>().AddRangeAsync(createResult).ConfigureAwait(false);

        var updateEntities = await handler.RunUpdateRangeHandlerAsync(entities).ConfigureAwait(false);
        var updateResult = updateEntities.Except(createResult, comparer).ToArray();

        if (updateResult.Any())
            context.Set<TEntity>().UpdateRange(updateResult);

        var processingResult = createResult.Concat(updateResult).ToArray();

        if (processingResult.Any())
        {
            await context.SaveChangesAsync().ConfigureAwait(false);

            if (createResult.Any())
                await handler.RunPostProcessAsync(RepositoryActions.Create, createResult).ConfigureAwait(false);
            if (updateResult.Any())
                await handler.RunPostProcessAsync(RepositoryActions.Create, updateResult).ConfigureAwait(false);
        }

        logger.LogDebug(nameof(CreateUpdateRangeAsync), info, $"Created: { createResult.Length}. Updated: { updateResult.Length}");

        return processingResult;
    }

    public async Task<TEntity[]> CreateUpdateDeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
            return Array.Empty<TEntity>();

        var createEntities = await handler.RunCreateRangeHandlerAsync(entities, comparer).ConfigureAwait(false);
        var createResult = createEntities.ToArray();

        if (createResult.Any())
            await context.Set<TEntity>().AddRangeAsync(createResult).ConfigureAwait(false);

        var updateEntities = await handler.RunUpdateRangeHandlerAsync(entities).ConfigureAwait(false);
        var updateResult = updateEntities.Except(createResult, comparer).ToArray();

        if (updateResult.Any())
            context.Set<TEntity>().UpdateRange(updateResult);

        var processingResult = createResult.Concat(updateResult).ToArray();

        var allData = await context.Set<TEntity>().ToArrayAsync().ConfigureAwait(false);
        var deleteResult = allData.Except(processingResult, comparer).ToArray();

        if (deleteResult.Any())
            context.Set<TEntity>().RemoveRange(deleteResult);

        if (processingResult.Any() || deleteResult.Any())
        {
            await context.SaveChangesAsync().ConfigureAwait(false);

            if (createResult.Any())
                await handler.RunPostProcessAsync(RepositoryActions.Create, createResult).ConfigureAwait(false);
            if (updateResult.Any())
                await handler.RunPostProcessAsync(RepositoryActions.Update, updateResult).ConfigureAwait(false);
            if (deleteResult.Any())
                await handler.RunPostProcessAsync(RepositoryActions.Delete, deleteResult).ConfigureAwait(false);
        }

        logger.LogDebug(nameof(CreateUpdateDeleteAsync), info, $"Created: {createResult.Length}. Updated: {updateResult.Length}. Deleted: {deleteResult.Length}");

        return processingResult;
    }

    public async Task<TEntity> DeleteAsync(object[] id, string info)
    {
        var result = await context.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
        
        if (result is null)
            throw new SqlNullValueException($"{typeof(TEntity).Name} with id: '{string.Join(",", id)}' not found");

        context.Set<TEntity>().Remove(result);
        await context.SaveChangesAsync().ConfigureAwait(false);
        await handler.RunPostProcessAsync(RepositoryActions.Delete, result);

        logger.LogDebug(nameof(DeleteAsync), info, OK);
        return result;
    }
    public async Task<TEntity[]> DeleteRangeAsync(IEnumerable<TEntity> entities, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
            return Array.Empty<TEntity>();

        var deleteResult = await handler.GetExist(entities).ToArrayAsync().ConfigureAwait(false);

        var count = 0;
        if (deleteResult.Any())
        {
            foreach (var entry in context.ChangeTracker.Entries<TEntity>())
                entry.State = EntityState.Detached;

            context.Set<TEntity>().RemoveRange(deleteResult);
            count = await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Delete, deleteResult);
        }

        logger.LogDebug(nameof(DeleteRangeAsync), info, count);

        return deleteResult;
    }

    public async Task<TEntity?> FindAsync(params object[] parameters) => await context.Set<TEntity>().FindAsync(parameters).ConfigureAwait(false);
    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);

    public IQueryable<TEntity> GetQuery() => context.Set<TEntity>();
    public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate) => context.Set<TEntity>().Where(predicate);

    public async Task<TEntity[]> GetSampleAsync(Expression<Func<TEntity, bool>> predicate, bool isTracking = false) => isTracking
        ? await context.Set<TEntity>().Where(predicate).ToArrayAsync().ConfigureAwait(false)
        : await context.Set<TEntity>().AsNoTracking().Where(predicate).ToArrayAsync().ConfigureAwait(false);
    public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool isTracking = false) => isTracking
        ? await context.Set<TEntity>().Select(selector).ToArrayAsync().ConfigureAwait(false)
        : await context.Set<TEntity>().AsNoTracking().Select(selector).ToArrayAsync().ConfigureAwait(false);
    public async Task<TResult[]> GetSampleAsync<TResult>(IQueryable<TEntity> query, Expression<Func<TEntity, TResult>> selector, bool isTracking = false) => isTracking
        ? await query.Select(selector).ToArrayAsync().ConfigureAwait(false)
        : await query.AsNoTracking().Select(selector).ToArrayAsync().ConfigureAwait(false);
    public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, bool isTracking = false) => isTracking
        ? await context.Set<TEntity>().Where(predicate).Select(selector).ToArrayAsync().ConfigureAwait(false)
        : await context.Set<TEntity>().AsNoTracking().Where(predicate).Select(selector).ToArrayAsync().ConfigureAwait(false);
    public async Task<TEntity[]> GetSampleOrderedAsync<TSelector>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TSelector>> orderSelector, Expression<Func<TEntity, TSelector>>? orderSelector2 = null, bool isTracking = false) => isTracking
        ? orderSelector2 is null
            ? await context.Set<TEntity>().Where(predicate).OrderBy(orderSelector).ToArrayAsync().ConfigureAwait(false)
            : await context.Set<TEntity>().Where(predicate).OrderBy(orderSelector).ThenBy(orderSelector2).ToArrayAsync().ConfigureAwait(false)
        : orderSelector2 is null
            ? await context.Set<TEntity>().AsNoTracking().Where(predicate).OrderBy(orderSelector).ToArrayAsync().ConfigureAwait(false)
            : await context.Set<TEntity>().AsNoTracking().Where(predicate).OrderBy(orderSelector).ThenBy(orderSelector2).ToArrayAsync().ConfigureAwait(false);

    public IQueryable<TEntity> GetPaginationQuery<TSelector>(Paginatior pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
        context.Set<TEntity>()
            .OrderBy(orderSelector)
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQuery<TSelector>(IQueryable<TEntity> query, Paginatior pagination, Expression<Func<TEntity, TSelector>> orderSelector, Expression<Func<TEntity, TSelector>>? orderSelector2 = null) =>
        orderSelector2 is null
        ? query.OrderBy(orderSelector).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit)
        : query.OrderBy(orderSelector).ThenBy(orderSelector2).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQueryDesc<TSelector>(Paginatior pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
        context.Set<TEntity>()
            .OrderByDescending(orderSelector)
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQueryDesc<TSelector>(IQueryable<TEntity> query, Paginatior pagination, Expression<Func<TEntity, TSelector>> orderSelector, Expression<Func<TEntity, TSelector>>? orderSelector2 = null) =>
        orderSelector2 is null
        ? query.OrderByDescending(orderSelector).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit)
        : query.OrderByDescending(orderSelector).ThenByDescending(orderSelector2).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);

    public async Task<int> GetCountAsync(IQueryable<TEntity> query) => await query.AsNoTracking().CountAsync().ConfigureAwait(false);
    public async Task<int> GetCountAsync() => await context.Set<TEntity>().AsNoTracking().CountAsync().ConfigureAwait(false);
}