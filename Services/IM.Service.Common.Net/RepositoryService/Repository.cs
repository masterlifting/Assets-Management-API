using IM.Service.Common.Net.HttpServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Common.Net.RepositoryService;

public class Repository<TEntity, TContext> where TEntity : class where TContext : DbContext
{
    private readonly TContext context;
    private readonly IRepositoryHandler<TEntity> handler;

    private readonly ILogger<Repository<TEntity, TContext>> logger;
    private readonly string entityName;

    private static string GetExceptionMessage(Exception exception) => exception.InnerException?.Message ?? exception.Message;

    protected Repository(
        ILogger<Repository<TEntity
        , TContext>> logger
        , TContext context
        , IRepositoryHandler<TEntity> handler)
    {
        this.logger = logger;
        this.context = context;
        this.handler = handler;
        entityName = typeof(TEntity).Name;
    }

    public async Task<(string? error, TEntity? result)> CreateAsync(TEntity entity, string info)
    {
        try
        {
            entity = await handler.RunCreateHandlerAsync(entity).ConfigureAwait(false);
            await context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Create, entity).ConfigureAwait(false);
            logger.LogInformation(LogEvents.Create, "Info: {info}. Entity: {name}", info, entityName);
            return (null, entity);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.Create, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
            return (message, null);
        }
    }
    public async Task<(string? error, TEntity[] result)> CreateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        try
        {
            entities = entities.ToArray();

            if (!entities.Any())
            {
                logger.LogWarning(LogEvents.Create, "{info}. Entity: {name}. No incoming collection", info, entityName);
                return (null, Array.Empty<TEntity>());
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

            logger.LogInformation(LogEvents.Create, "Info: {info}. Entity: {name}. Created: {count}", info, entityName, count);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.Create, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
            return (message, Array.Empty<TEntity>());
        }
    }

    public async Task<(string? error, TEntity? result)> UpdateAsync(object[] id, TEntity entity, string info)
    {
        try
        {
            entity = await handler.RunUpdateHandlerAsync(id, entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);
            logger.LogInformation(LogEvents.Update, "Info: {info}. Entity: {name}", info, entityName);
            return (null, entity);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.Update, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
            return (message, null);
        }
    }
    public async Task<(string? error, TEntity? result)> UpdateAsync(TEntity entity, string info)
    {
        try
        {
            entity = await handler.RunUpdateHandlerAsync(entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);
            logger.LogInformation(LogEvents.Update, "Info: {info}. Entity: {name}", info, entityName);
            return (null, entity);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.Update, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
            return (message, null);
        }

    }
    public async Task<(string? error, TEntity[] result)> UpdateAsync(IEnumerable<TEntity> entities, string info)
    {
        try
        {
            entities = entities.ToArray();

            if (!entities.Any())
            {
                logger.LogWarning(LogEvents.Update, "{info}. Entity: {name}. No incoming collection", info, entityName);
                return (null, Array.Empty<TEntity>());
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

            logger.LogInformation(LogEvents.Update, "Info: {info}. Entity: {name}. Updated: {count}", info, entityName, count);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.Update, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
            return (message, Array.Empty<TEntity>());
        }
    }

    public async Task<(string? error, TEntity? result)> CreateUpdateAsync(object[] id, TEntity entity, string info)
    {
        try
        {
            entity = await handler.RunCreateHandlerAsync(entity).ConfigureAwait(false);
            await context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Create, entity).ConfigureAwait(false);
            logger.LogInformation(LogEvents.Create, "Info: {info}. Entity: {name}", info, entityName);
            return (null, entity);
        }
        catch
        {
            try
            {
                entity = await handler.RunUpdateHandlerAsync(id, entity).ConfigureAwait(false);
                context.Entry(entity).State = EntityState.Modified;
                context.Set<TEntity>().Update(entity);
                await context.SaveChangesAsync().ConfigureAwait(false);
                await handler.RunPostProcessAsync(RepositoryActions.Update, entity).ConfigureAwait(false);
                logger.LogInformation(LogEvents.Update, "Info: {info}. Entity: {name}", info, entityName);
                return (null, entity);
            }
            catch (Exception exception)
            {
                var message = GetExceptionMessage(exception);
                logger.LogError(LogEvents.Update, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
                return (message, null);
            }
        }
    }
    public async Task<(string? error, TEntity[] result)> CreateUpdateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        try
        {
            entities = entities.ToArray();

            if (!entities.Any())
            {
                logger.LogWarning(LogEvents.CreateUpdate, "Info: {info}. Entity: {name}. No incoming collection", info, entityName);
                return (null, Array.Empty<TEntity>());
            }

            var createEntities = await handler.RunCreateRangeHandlerAsync(entities, comparer).ConfigureAwait(false);
            var createResult = createEntities.ToArray();

            if (createResult.Any())
                await context.Set<TEntity>().AddRangeAsync(createResult).ConfigureAwait(false);

            var updateEntities = await handler.RunUpdateRangeHandlerAsync(entities).ConfigureAwait(false);
            var updateResult = updateEntities.ToArray();

            if (updateResult.Any())
                context.Set<TEntity>().UpdateRange(updateResult);

            var processing = createResult.Concat(updateResult).ToArray();

            if (processing.Any())
            {
                await context.SaveChangesAsync().ConfigureAwait(false);

                if (createResult.Any())
                    await handler.RunPostProcessAsync(RepositoryActions.Create, createResult).ConfigureAwait(false);
                if (updateResult.Any())
                    await handler.RunPostProcessAsync(RepositoryActions.Create, updateResult).ConfigureAwait(false);
            }

            logger.LogInformation(LogEvents.CreateUpdate, "Info: {info}. Entity: {name}. Created: {ccount}. Updated: {ucount}", info, entityName, createResult.Length, updateResult.Length);
            return (null, processing);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.CreateUpdate, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
            return (message, Array.Empty<TEntity>());
        }
    }

    public async Task<(string? error, TEntity[] result)> CreateUpdateDeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        try
        {
            entities = entities.ToArray();

            if (!entities.Any())
            {
                logger.LogWarning(LogEvents.CreateUpdateDelete, "{info}. Entity: {name}. No incoming collection", info, entityName);
                return (null, Array.Empty<TEntity>());
            }

            var createEntities = await handler.RunCreateRangeHandlerAsync(entities, comparer).ConfigureAwait(false);
            var createResult = createEntities.ToArray();

            if (createResult.Any())
                await context.Set<TEntity>().AddRangeAsync(createResult).ConfigureAwait(false);

            var updateEntities = await handler.RunUpdateRangeHandlerAsync(entities).ConfigureAwait(false);
            var updateResult = updateEntities.ToArray();

            if (updateResult.Any())
                context.Set<TEntity>().UpdateRange(updateResult);

            var processingResult = createResult.Concat(updateResult).ToArray();

            var deleteEntities = await handler.RunDeleteRangeHandlerAsync(processingResult).ConfigureAwait(false);
            var deleteResult = deleteEntities.ToArray();

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

            logger.LogInformation(LogEvents.CreateUpdateDelete, "Info: {info}. Entity: {name}. Created: {ccount}. Updated: {ucount}. Deleted: {dcount}", info, entityName, createResult.Length, updateResult.Length, deleteResult.Length);
            return (null, processingResult);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.CreateUpdateDelete, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
            return (message, Array.Empty<TEntity>());
        }
    }

    public async Task<(string? error, TEntity? entity)> DeleteByIdAsync(object[] id, string info)
    {
        try
        {
            var result = await handler.RunDeleteHandlerAsync(id).ConfigureAwait(false);
            context.Set<TEntity>().Remove(result);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Delete, result);
            logger.LogInformation(LogEvents.Delete, "Info: {info}. Entity: {name}", info, entityName);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.Delete, "Info: {info}: Error: {exception}", info, message);
            return (message, null);
        }
    }
    public async Task<(string? error, TEntity? result)> DeleteAsync(TEntity entity, string info)
    {
        try
        {
            var result = await handler.RunDeleteHandlerAsync(entity).ConfigureAwait(false);
            context.Set<TEntity>().Remove(result);
            await context.SaveChangesAsync().ConfigureAwait(false);
            await handler.RunPostProcessAsync(RepositoryActions.Delete, result);
            logger.LogInformation(LogEvents.Delete, "Info: {info}. Entity: {name}", info, entityName);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.Delete, "Info: {info}: Error: {exception}", info, message);
            return (message, null);
        }
    }
    public async Task<(string? error, TEntity[] result)> DeleteAsync(IEnumerable<TEntity> entities, string info)
    {
        try
        {
            entities = entities.ToArray();

            if (!entities.Any())
            {
                logger.LogWarning(LogEvents.Delete, "{info}. Entity: {name}. No incoming collection", info, entityName);
                return (null, Array.Empty<TEntity>());
            }

            var deleteResult = await handler.GetExist(entities).ToArrayAsync().ConfigureAwait(false);

            var count = 0;
            if (deleteResult.Any())
            {
                context.Set<TEntity>().RemoveRange(deleteResult);
                count = await context.SaveChangesAsync().ConfigureAwait(false);
                await handler.RunPostProcessAsync(RepositoryActions.Delete, deleteResult);
            }

            logger.LogInformation(LogEvents.Delete, "Info: {info}. Entity: {name}. Deleted: {count}", info, entityName, count);
            return (null, deleteResult);
        }
        catch (Exception exception)
        {
            var message = GetExceptionMessage(exception);
            logger.LogError(LogEvents.Delete, "Info: {info}. Entity: {name}. Error: {exception}", info, entityName, message);
            return (message, Array.Empty<TEntity>());
        }
    }

    public DbSet<TEntity> GetDbSet() => context.Set<TEntity>();

    public async Task<TEntity?> FindAsync(params object[] parameters) => await context.Set<TEntity>().FindAsync(parameters).ConfigureAwait(false);
    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().FirstOrDefaultAsync(predicate).ConfigureAwait(false);

    public IQueryable<TEntity> GetQuery() => context.Set<TEntity>();
    public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate) => context.Set<TEntity>().Where(predicate);

    public IQueryable<TEntity> GetPaginationQuery<TSelector>(HttpPagination pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
        context.Set<TEntity>()
            .OrderBy(orderSelector)
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQueryDesc<TSelector>(HttpPagination pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
        context.Set<TEntity>()
            .OrderByDescending(orderSelector)
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQuery<TSelector>(IQueryable<TEntity> query, HttpPagination pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
        query
            .OrderBy(orderSelector)
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQuery<TSelector1, TSelector2>(IQueryable<TEntity> query, HttpPagination pagination, Expression<Func<TEntity, TSelector1>> orderSelector1, Expression<Func<TEntity, TSelector2>> orderSelector2) =>
        query.OrderBy(orderSelector1).ThenBy(orderSelector2).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);

    public async Task<TEntity[]> GetSampleAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().Where(predicate).ToArrayAsync().ConfigureAwait(false);
    public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, TResult>> selector) =>
        await context.Set<TEntity>().AsNoTracking().Select(selector).ToArrayAsync().ConfigureAwait(false);
    public async Task<TResult[]> GetSampleAsync<TResult>(IQueryable<TEntity> query, Expression<Func<TEntity, TResult>> selector) =>
        await query.AsNoTracking().Select(selector).ToArrayAsync().ConfigureAwait(false);
    public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector) =>
        await context.Set<TEntity>().AsNoTracking().Where(predicate).Select(selector).ToArrayAsync().ConfigureAwait(false);
    public async Task<TEntity[]> GetSampleOrderedAsync<TSelector>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TSelector>> orderSelector) =>
        await context.Set<TEntity>().AsNoTracking().Where(predicate).OrderBy(orderSelector).ToArrayAsync().ConfigureAwait(false);

    public async Task<int> GetCountAsync(IQueryable<TEntity> query) => await query.AsNoTracking().CountAsync().ConfigureAwait(false);
    public async Task<int> GetCountAsync() => await context.Set<TEntity>().AsNoTracking().CountAsync().ConfigureAwait(false);
}