using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
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
    private const string NoCollection = "No incomming collection";

    private readonly TContext context;
    private readonly IRepositoryHandler<TEntity> handler;

    private readonly ILogger<Repository<TEntity, TContext>> logger;

    protected Repository(
        ILogger<Repository<TEntity
        , TContext>> logger
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

        logger.LogDebug(nameof(CreateAsync), OK, info);

        return entity;
    }
    public async Task<TEntity[]> CreateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
        {
            logger.LogDebug(nameof(CreateAsync), NoCollection, info);
            return Array.Empty<TEntity>();
        }

        entities = await handler.RunCreateRangeHandlerAsync(entities, comparer).ConfigureAwait(false);

        var result = entities.ToArray();
        var count = result.Length;

        if (result.Any())
        {
            await context.Set<TEntity>().AddRangeAsync(result).ConfigureAwait(false);
            count = await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Create, result).ConfigureAwait(false);
        }

        logger.LogDebug(nameof(CreateAsync), count, info);

        return result;
    }

    public async Task<TEntity> UpdateAsync(object[] id, TEntity entity, string info)
    {
        entity = await handler.RunUpdateHandlerAsync(id, entity).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
        await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);

        logger.LogDebug(nameof(UpdateAsync), OK, info);

        return entity;
    }
    public async Task<TEntity> UpdateAsync(TEntity entity, string info)
    {
        entity = await handler.RunUpdateHandlerAsync(entity).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
        await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);

        logger.LogDebug(nameof(UpdateAsync), OK, info);

        return entity;
    }
    public async Task<TEntity[]> UpdateAsync(IEnumerable<TEntity> entities, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
        {
            logger.LogDebug(nameof(UpdateAsync), NoCollection, info);
            return Array.Empty<TEntity>();
        }

        entities = await handler.RunUpdateRangeHandlerAsync(entities).ConfigureAwait(false);

        var result = entities.ToArray();
        var count = result.Length;

        if (result.Any())
        {
            context.Set<TEntity>().UpdateRange(result);
            count = await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Update, result).ConfigureAwait(false);
        }

        logger.LogDebug(nameof(UpdateAsync), count, info);

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

            logger.LogDebug(nameof(CreateUpdateAsync), "Created: " + OK, info);
        }
        catch
        {
            entity = await handler.RunUpdateHandlerAsync(id, entity).ConfigureAwait(false);
            context.Entry(entity).State = EntityState.Modified;
            context.Set<TEntity>().Update(entity);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);

            logger.LogDebug(nameof(CreateUpdateAsync), "Updated: " + OK, info);
        }
        return entity;
    }
    public async Task<TEntity[]> CreateUpdateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
        {
            logger.LogDebug(nameof(CreateUpdateAsync), NoCollection, info);
            return Array.Empty<TEntity>();
        }

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

        logger.LogDebug(nameof(CreateUpdateAsync), $"Created: { createResult.Length}. Updated: { updateResult.Length}", info);

        return processingResult;
    }

    public async Task<TEntity[]> CreateUpdateDeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
        {
            logger.LogDebug(nameof(CreateUpdateDeleteAsync), NoCollection, info);
            return Array.Empty<TEntity>();
        }

        var createEntities = await handler.RunCreateRangeHandlerAsync(entities, comparer).ConfigureAwait(false);
        var createResult = createEntities.ToArray();

        if (createResult.Any())
            await context.Set<TEntity>().AddRangeAsync(createResult).ConfigureAwait(false);

        var updateEntities = await handler.RunUpdateRangeHandlerAsync(entities).ConfigureAwait(false);
        var updateResult = updateEntities.Except(createResult, comparer).ToArray();

        if (updateResult.Any())
            context.Set<TEntity>().UpdateRange(updateResult);

        var processingResult = createResult.Concat(updateResult).ToArray();

        var deleteEntities = await handler.RunDeleteRangeHandlerAsync(processingResult).ConfigureAwait(false);
        var deleteResult = deleteEntities.Except(processingResult, comparer).ToArray();

        if (deleteResult.Any())
            context.Set<TEntity>().RemoveRange(deleteResult);

        if (processingResult.Any() || deleteResult.Any())
        {
            await context.SaveChangesAsync().ConfigureAwait(false);

            if (createResult.Any())
                await handler.RunPostProcessAsync(RepositoryActions.Create, createResult).ConfigureAwait(false);
            if (updateResult.Any())
                await handler.RunPostProcessAsync(RepositoryActions.Create, updateResult).ConfigureAwait(false);
            if (deleteResult.Any())
                await handler.RunPostProcessAsync(RepositoryActions.Delete, deleteResult).ConfigureAwait(false);
        }

        logger.LogDebug(nameof(CreateUpdateDeleteAsync), $"Created: {createResult.Length}. Updated: {updateResult.Length}. Deleted: {deleteResult.Length}", info);

        return processingResult;
    }

    public async Task<TEntity> DeleteByIdAsync(object[] id, string info)
    {
        var result = await handler.RunDeleteHandlerAsync(id).ConfigureAwait(false);
        context.Set<TEntity>().Remove(result);
        await context.SaveChangesAsync().ConfigureAwait(false);
        await handler.RunPostProcessAsync(RepositoryActions.Delete, result);

        logger.LogDebug(nameof(DeleteByIdAsync), OK, info);
        return result;
    }
    public async Task<TEntity> DeleteAsync(TEntity entity, string info)
    {
        var result = await handler.RunDeleteHandlerAsync(entity).ConfigureAwait(false);
        context.Set<TEntity>().Remove(result);
        await context.SaveChangesAsync().ConfigureAwait(false);
        await handler.RunPostProcessAsync(RepositoryActions.Delete, result);

        logger.LogDebug(nameof(DeleteAsync), OK, info);

        return result;
    }
    public async Task<TEntity[]> DeleteAsync(IEnumerable<TEntity> entities, string info)
    {
        entities = entities.ToArray();

        if (!entities.Any())
        {
            logger.LogDebug(nameof(DeleteAsync), NoCollection, info);
            return Array.Empty<TEntity>();
        }

        var deleteResult = await handler.GetExist(entities).ToArrayAsync().ConfigureAwait(false);

        var count = 0;
        if (deleteResult.Any())
        {
            context.Set<TEntity>().RemoveRange(deleteResult);
            count = await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Delete, deleteResult);
        }

        logger.LogDebug(nameof(DeleteAsync), count, info);

        return deleteResult;
    }

    public DbSet<TEntity> GetDbSet() => context.Set<TEntity>();

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