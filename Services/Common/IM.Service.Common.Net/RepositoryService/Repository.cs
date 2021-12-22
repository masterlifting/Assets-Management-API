using IM.Service.Common.Net.HttpServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IM.Service.Common.Net.RepositoryService;

public class Repository<TEntity, TContext> where TEntity : class where TContext : DbContext
{
    private readonly TContext context;
    private readonly IRepositoryHandler<TEntity> handler;
    private readonly ILogger<Repository<TEntity, TContext>> logger;
    private readonly string name;

    protected Repository(
        ILogger<Repository<TEntity
        , TContext>> logger
        , TContext context
        , IRepositoryHandler<TEntity> handler)
    {
        this.logger = logger;
        this.context = context;
        this.handler = handler;
        name = typeof(TEntity).Name;
    }

    public async Task<(string? error, TEntity? result)> CreateAsync(TEntity entity, string info)
    {
        try
        {
            entity = await handler.GetCreateHandlerAsync(entity);
            await context.Set<TEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
            await handler.SetPostProcessAsync(entity);
            logger.LogInformation(LogEvents.Create, "{info}. Entity: {name}", info, name);
            return (null, entity);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Create, "{info}. Entity: {name}. Error: {exception}", info, name, message);
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
                logger.LogWarning(LogEvents.Create, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            entities = await handler.GetCreateRangeHandlerAsync(entities, comparer);

            var result = entities.ToArray();
            var count = result.Length;

            if (result.Any())
            {
                await context.Set<TEntity>().AddRangeAsync(result);
                count = await context.SaveChangesAsync();
                await handler.SetPostProcessAsync(result);
            }

            logger.LogInformation(LogEvents.Create, "{info}. Entity: {name}. Processed count: {count}", info, name, count);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Create, "{info}. Entity: {name}. Error: {exception}", info, name, message);
            return (message, Array.Empty<TEntity>());
        }
    }

    public async Task<(string? error, TEntity? result)> UpdateAsync(object[] id, TEntity entity, string info)
    {
        try
        {
            entity = await handler.GetUpdateHandlerAsync(id, entity);
            await context.SaveChangesAsync();
            await handler.SetPostProcessAsync(entity);
            logger.LogInformation(LogEvents.Update, "{info}. Entity: {name}", info, name);
            return (null, entity);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Update, "{info}. Entity: {name}. Error: {exception}", info, name, message);
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
                logger.LogWarning(LogEvents.Update, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            entities = await handler.GetUpdateRangeHandlerAsync(entities);

            var result = entities.ToArray();
            var count = result.Length;

            if (result.Any())
            {
                context.Set<TEntity>().UpdateRange(result);
                count = await context.SaveChangesAsync();
                await handler.SetPostProcessAsync(result);
            }

            logger.LogInformation(LogEvents.Update, "{info}. Entity: {name}. Processed count: {count}", info, name, count);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Update, "{info}. Entity: {name}. Error: {exception}", info, name, message);
            return (message, Array.Empty<TEntity>());
        }
    }

    public async Task<(string? error, TEntity? result)> CreateUpdateAsync(object[] id, TEntity entity, string info)
    {
        try
        {
            entity = await handler.GetCreateHandlerAsync(entity);
            await context.Set<TEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
            await handler.SetPostProcessAsync(entity);
            logger.LogInformation(LogEvents.Create, "{info}. Entity: {name}", info, name);
            return (null, entity);
        }
        catch
        {
            try
            {
                entity = await handler.GetUpdateHandlerAsync(id, entity);
                context.Entry(entity).State = EntityState.Modified;
                context.Set<TEntity>().Update(entity);
                await context.SaveChangesAsync();
                await handler.SetPostProcessAsync(entity);
                logger.LogInformation(LogEvents.Update, "{info}. Entity: {name}", info, name);
                return (null, entity);
            }
            catch (Exception updateException)
            {
                var message = updateException.InnerException?.Message ?? updateException.Message;
                logger.LogError(LogEvents.Update, "{info}. Entity: {name}. Error: {exception}", info, name, message);
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
                logger.LogWarning(LogEvents.CreateUpdate, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            var createEntities = await handler.GetCreateRangeHandlerAsync(entities, comparer);
            var createResult = createEntities.ToArray();

            if (createResult.Any())
                await context.Set<TEntity>().AddRangeAsync(createResult);

            var updateEntities = entities.Except(createResult, comparer);
            var updateResult = updateEntities.ToArray();

            if (updateResult.Any())
            {
                updateEntities = await handler.GetUpdateRangeHandlerAsync(updateResult);
                updateResult = updateEntities.ToArray();

                if (updateResult.Any())
                    context.Set<TEntity>().UpdateRange(updateResult);
            }

            var result = createResult.Concat(updateResult).ToArray();

            if (result.Any())
            {
                await context.SaveChangesAsync();
                await handler.SetPostProcessAsync(result);
            }

            logger.LogInformation(LogEvents.CreateUpdate, "{info}. Entity: {name}. Processed count: {count}", info, name, result.Length);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.CreateUpdate, "{info}. Entity: {name}. Error: {exception}", info, name, message);
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
                logger.LogWarning(LogEvents.CreateUpdateDelete, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            var createEntities = await handler.GetCreateRangeHandlerAsync(entities, comparer);
            var createResult = createEntities.ToArray();

            if (createResult.Any())
                await context.Set<TEntity>().AddRangeAsync(createResult);

            var updateEntities = entities.Except(createResult, comparer);
            var updateResult = updateEntities.ToArray();

            if (updateResult.Any())
            {
                updateEntities = await handler.GetUpdateRangeHandlerAsync(updateResult);
                updateResult = updateEntities.ToArray();

                if (updateResult.Any())
                    context.Set<TEntity>().UpdateRange(updateResult);
            }

            var processingResult = createResult.Concat(updateResult).ToArray();

            var deleteEntities = await handler.GetDeleteRangeHandlerAsync(processingResult);
            var deleteResult = deleteEntities.ToArray();

            if (deleteResult.Any())
                context.Set<TEntity>().RemoveRange(deleteResult);

            if (processingResult.Any() || deleteResult.Any())
            {
                await context.SaveChangesAsync();
                await handler.SetPostProcessAsync(processingResult.Concat(deleteResult).ToArray());
            }

            logger.LogInformation(LogEvents.CreateUpdateDelete, "{info}. Entity: {name}. Processed count: {pcount}. Deleted count: {dcount}", info, name, processingResult.Length, deleteResult.Length);
            return (null, processingResult);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.CreateUpdateDelete, "{info}. Entity: {name}. Error: {exception}", info, name, message);
            return (message, Array.Empty<TEntity>());
        }
    }
    public async Task<string?> ReCreateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        try
        {
            entities = entities.ToArray();

            if (!entities.Any())
            {
                logger.LogWarning(LogEvents.ReCreate, "{info}. Entity: {name}. No incoming collection", info, name);
                return null;
            }

            var oldData = context.Set<TEntity>();
            context.Set<TEntity>().RemoveRange(oldData);
            await context.SaveChangesAsync();

            var newData = await handler.GetCreateRangeHandlerAsync(entities, comparer);
            var result = newData.ToArray();

            if (result.Any())
            {
                await context.Set<TEntity>().AddRangeAsync(result);
                await context.SaveChangesAsync();
                await handler.SetPostProcessAsync(result);
            }

            logger.LogInformation(LogEvents.ReCreate, "{info}. Entity: {name}. Processed count: {count}", info, name, result.Length);
            return null;
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.ReCreate, "{info}. Entity: {name}. Error: {exception}", info, name, message);
            return message;
        }
    }

    public async Task<string?> DeleteAsync(string info)
    {
        try
        {
            context.Set<TEntity>().RemoveRange(context.Set<TEntity>());
            await context.SaveChangesAsync();

            logger.LogInformation(LogEvents.Remove, info);
            return null;
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Remove, "{info}: Error: {exception}", info, message);
            return message;
        }
    }
    public async Task<(string? error, TEntity? entity)> DeleteAsync(object[] id, string info)
    {
        try
        {
            var result = await handler.GetDeleteHandlerAsync(id);
            context.Set<TEntity>().Remove(result);
            await context.SaveChangesAsync();
            await handler.SetPostProcessAsync(result);
            logger.LogInformation(LogEvents.Remove, info);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Remove, "{info}: Error: {exception}", info, message);
            return (message, null);
        }
    }
    public async Task<(string? error, TEntity[] result)> DeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        try
        {
            entities = entities.ToArray();

            if (!entities.Any())
            {
                logger.LogWarning(LogEvents.Remove, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            var deleteResult = context.Set<TEntity>().Intersect(entities, comparer).ToArray();

            var count = 0;
            if (deleteResult.Any())
            {
                context.Set<TEntity>().RemoveRange(deleteResult);
                count = await context.SaveChangesAsync();
                await handler.SetPostProcessAsync(deleteResult);
            }

            logger.LogInformation(LogEvents.Remove, "{info}. Entity: {name}. Processed count: {count}", info, name, count);
            return (null, deleteResult);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Remove, "{info}. Entity: {name}. Error: {exception}", info, name, message);
            return (message, Array.Empty<TEntity>());
        }
    }

    public DbSet<TEntity> GetDbSet() => context.Set<TEntity>();

    public async Task<TEntity?> FindAsync(params object[] parameters) => await context.Set<TEntity>().FindAsync(parameters);
    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(predicate);
    public async Task<TEntity?> FindLastAsync<TSelector>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TSelector>> orderSelector) =>
        await context.Set<TEntity>().OrderBy(orderSelector).LastOrDefaultAsync(predicate);
    public async Task<TEntity?> FindFirstAsync<TSelector>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TSelector>> orderSelector) =>
        await context.Set<TEntity>().OrderBy(orderSelector).FirstOrDefaultAsync(predicate);
    public async Task<TEntity?> FindFirstAsync<TSelector1, TSelector2>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TSelector1>> orderSelector1, Expression<Func<TEntity, TSelector2>> orderSelector2) =>
        await context.Set<TEntity>().OrderBy(orderSelector1).ThenBy(orderSelector2).FirstOrDefaultAsync(predicate);

    public IQueryable<TEntity> GetQuery() => context.Set<TEntity>();
    public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate) => context.Set<TEntity>().Where(predicate);
    public IQueryable<TEntity> GetQuery(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> predicate) => query.Where(predicate);

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
    public IQueryable<TEntity> GetPaginationQuery<TSelector1, TSelector2>(HttpPagination pagination, Expression<Func<TEntity, TSelector1>> orderSelector1, Expression<Func<TEntity, TSelector2>> orderSelector2) =>
        context.Set<TEntity>()
            .OrderBy(orderSelector1)
            .ThenBy(orderSelector2)
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQuery<TSelector>(IQueryable<TEntity> query, HttpPagination pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
        query
            .OrderBy(orderSelector)
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQuery<TSelector1, TSelector2>(IQueryable<TEntity> query, HttpPagination pagination, Expression<Func<TEntity, TSelector1>> orderSelector1, Expression<Func<TEntity, TSelector2>> orderSelector2) =>
        query.OrderBy(orderSelector1).ThenBy(orderSelector2).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);

    public async Task<TEntity[]> GetSampleAsync() => await context.Set<TEntity>().AsNoTracking().ToArrayAsync();
    public async Task<TEntity[]> GetSampleAsync(IQueryable<TEntity> query) => await query.AsNoTracking().ToArrayAsync();
    public async Task<TEntity[]> GetSampleAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().Where(predicate).ToArrayAsync();
    public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, TResult>> selector) =>
        await context.Set<TEntity>().AsNoTracking().Select(selector).ToArrayAsync();
    public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector) =>
        await context.Set<TEntity>().AsNoTracking().Where(predicate).Select(selector).ToArrayAsync();
    public async Task<TEntity[]> GetSampleOrderedAsync<TSelector>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TSelector>> orderSelector) =>
        await context.Set<TEntity>().AsNoTracking().Where(predicate).OrderBy(orderSelector).ToArrayAsync();

    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().CountAsync(predicate);
    public async Task<int> GetCountAsync(IQueryable<TEntity> query) => await query.AsNoTracking().CountAsync();
    public async Task<int> GetCountAsync() => await context.Set<TEntity>().AsNoTracking().CountAsync();

    public async Task<bool> GetAnyAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().AnyAsync(predicate);
}