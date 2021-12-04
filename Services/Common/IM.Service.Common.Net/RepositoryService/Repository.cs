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
            await handler.GetCreateHandlerAsync(ref entity);
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
    public async Task<(string? error, TEntity[]? result)> CreateAsync(IEnumerable<TEntity> entities, string info)
    {
        try
        {
            var result = entities as TEntity[] ?? entities.ToArray();

            if (!result.Any())
            {
                logger.LogWarning(LogEvents.Create, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            var count = 0;
            await handler.GetCreateHandlerAsync(ref result);

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
            return (message, null);
        }
    }

    public async Task<(string? error, TEntity? result)> UpdateAsync(TEntity entity, string info)
    {
        try
        {
            await handler.GetUpdateHandlerAsync(ref entity);
            context.Set<TEntity>().Update(entity);
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
    public async Task<(string? error, TEntity[]? result)> UpdateAsync(IEnumerable<TEntity> entities, string info)
    {
        try
        {
            var result = entities as TEntity[] ?? entities.ToArray();

            if (!result.Any())
            {
                logger.LogWarning(LogEvents.Update, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            var count = 0;
            await handler.GetUpdateHandlerAsync(ref result);

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
            return (message, null);
        }
    }

    public async Task<(string? error, TEntity? result)> CreateUpdateAsync(TEntity entity, string info)
    {
        try
        {
            await handler.GetCreateHandlerAsync(ref entity);
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
                await handler.GetUpdateHandlerAsync(ref entity);
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
    public async Task<(string? error, TEntity[]? result)> CreateUpdateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        try
        {
            var result = entities as TEntity[] ?? entities.ToArray();

            if (!result.Any())
            {
                logger.LogWarning(LogEvents.CreateUpdate, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            var createResult = result.ToArray();

            await handler.GetCreateHandlerAsync(ref createResult);

            if (createResult.Any())
                await context.Set<TEntity>().AddRangeAsync(createResult);

            var updateResult = result.Except(createResult, comparer).ToArray();

            if (updateResult.Any())
            {
                await handler.GetUpdateHandlerAsync(ref updateResult);
                context.Set<TEntity>().UpdateRange(updateResult);
            }

            result = createResult.Concat(updateResult).ToArray();

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
            return (message, null);
        }
    }

    public async Task<(string? error, TEntity[]? result)> CreateUpdateDeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        try
        {
            var result = entities as TEntity[] ?? entities.ToArray();

            if (!result.Any())
            {
                logger.LogWarning(LogEvents.CreateUpdateDelete, "{info}. Entity: {name}. No incoming collection", info, name);
                return (null, Array.Empty<TEntity>());
            }

            var createResult = result.ToArray();

            await handler.GetCreateHandlerAsync(ref createResult);

            if (createResult.Any())
                await context.Set<TEntity>().AddRangeAsync(createResult);

            var updateResult = result.Except(createResult, comparer).ToArray();

            if (updateResult.Any())
            {
                await handler.GetUpdateHandlerAsync(ref updateResult);
                context.Set<TEntity>().UpdateRange(updateResult);
            }

            result = createResult.Concat(updateResult).ToArray();

            var deleteResult = await handler.GetDeleteHandlerAsync(result);

            if (deleteResult.Any())
                context.Set<TEntity>().RemoveRange(deleteResult);

            var processResult = result.Concat(deleteResult).ToArray();

            if (processResult.Any())
            {
                await context.SaveChangesAsync();
                await handler.SetPostProcessAsync(processResult);
            }

            logger.LogInformation(LogEvents.CreateUpdateDelete, "{info}. Entity: {name}. Processed count: {pcount}. Deleted count: {dcount}", info, name, result.Length, deleteResult.Count);
            return (null, result);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.CreateUpdateDelete, "{info}. Entity: {name}. Error: {exception}", info, name, message);
            return (message, null);
        }
    }
    public async Task<string?> ReCreateAsync(IEnumerable<TEntity> entities, string info)
    {
        try
        {
            var result = entities as TEntity[] ?? entities.ToArray();

            if (!result.Any())
            {
                logger.LogWarning(LogEvents.ReCreate, "{info}. Entity: {name}. No incoming collection", info, name);
                return null;
            }

            var old = context.Set<TEntity>();
            context.Set<TEntity>().RemoveRange(old);
            await context.SaveChangesAsync();

            await handler.GetCreateHandlerAsync(ref result);
            if (result.Any())
            {
                await context.Set<TEntity>().AddRangeAsync(result);
                await handler.SetPostProcessAsync(result);
                await context.SaveChangesAsync();
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
    public async Task<(string? error, TEntity? entity)> DeleteAsync(string info, params object[] id)
    {
        try
        {
            var entity = await context.Set<TEntity>().FindAsync(id);

            if (entity is null)
                throw new NullReferenceException(nameof(entity));

            context.Set<TEntity>().Remove(entity);

            await context.SaveChangesAsync();

            await handler.SetPostProcessAsync(entity);

            logger.LogInformation(LogEvents.Remove, info);
            return (null, entity);
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Remove, "{info}: Error: {exception}", info, message);
            return (message, null);
        }
    }
    public async Task<string?> DeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
    {
        try
        {
            var result = entities as TEntity[] ?? entities.ToArray();

            if (!result.Any())
            {
                logger.LogWarning(LogEvents.Remove, "{info}. Entity: {name}. No incoming collection", info, name);
                return null;
            }

            var deleteResult = context.Set<TEntity>().Intersect(result, comparer).ToArray();
            context.Set<TEntity>().RemoveRange(deleteResult);

            var count = await context.SaveChangesAsync();

            if (count > 0)
                await handler.SetPostProcessAsync(deleteResult);

            logger.LogInformation(LogEvents.Remove, "{info}. Entity: {name}. Processed count: {count}", info, name, count);
            return null;
        }
        catch (Exception exception)
        {
            var message = exception.InnerException?.Message ?? exception.Message;
            logger.LogError(LogEvents.Remove, "{info}. Entity: {name}. Error: {exception}", info, name, message);
            return message;
        }
    }

    public DbSet<TEntity> GetDbSet() => context.Set<TEntity>();

    public async Task<TEntity?> FindAsync(params object[] parameters) => await context.Set<TEntity>().FindAsync(parameters);
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
    public IQueryable<TEntity> GetPaginationQuery<TSelector1, TSelector2>(HttpPagination pagination, Expression<Func<TEntity, TSelector1>> orderSelector1, Expression<Func<TEntity, TSelector2>> orderSelector2) =>
        context.Set<TEntity>()
            .OrderBy(orderSelector1)
            .ThenBy(orderSelector2)
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQuery<TSelector>(IQueryable<TEntity> query, HttpPagination pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
        query.OrderBy(orderSelector).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);
    public IQueryable<TEntity> GetPaginationQuery<TSelector1, TSelector2>(IQueryable<TEntity> query, HttpPagination pagination, Expression<Func<TEntity, TSelector1>> orderSelector1, Expression<Func<TEntity, TSelector2>> orderSelector2) =>
        query.OrderBy(orderSelector1).ThenBy(orderSelector2).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);

    public async Task<TEntity[]> GetSampleAsync() => await context.Set<TEntity>().AsNoTracking().ToArrayAsync();
    public async Task<TEntity[]> GetSampleAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().Where(predicate).ToArrayAsync();
    public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, TResult>> selector) =>
        await context.Set<TEntity>().Select(selector).ToArrayAsync();
    public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector) =>
        await context.Set<TEntity>().Where(predicate).Select(selector).ToArrayAsync();
    public async Task<TEntity[]> GetSampleOrderedAsync<TSelector>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TSelector>> orderSelector) =>
        await context.Set<TEntity>().AsNoTracking().Where(predicate).OrderBy(orderSelector).ToArrayAsync();

    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().CountAsync(predicate);
    public async Task<int> GetCountAsync(IQueryable<TEntity> query) => await query.AsNoTracking().CountAsync();
    public async Task<int> GetCountAsync() => await context.Set<TEntity>().AsNoTracking().CountAsync();

    public async Task<bool> GetAnyAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().AnyAsync(predicate);
}