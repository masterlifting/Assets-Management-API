using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using IM.Service.Common.Net.HttpServices;

namespace IM.Service.Common.Net.RepositoryService
{
    public class Repository<TEntity, TContext> where TEntity : class where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IRepositoryHandler<TEntity> handler;

        protected Repository(TContext context, IRepositoryHandler<TEntity> handler)
        {
            this.context = context;
            this.handler = handler;
        }

        public async Task<(string? error, TEntity? result)> CreateAsync(TEntity entity, string info)
        {
            try
            {
                await handler.GetCreateHandlerAsync(ref entity);
                await context.Set<TEntity>().AddAsync(entity);
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.Create, NotifyType.Error, info, exception);
                return (exception.Message, null);
            }

            SetInfo(ActionType.Create, NotifyType.Success, info);
            return (null, entity);
        }
        public async Task<(string? error, TEntity[]? result)> CreateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            TEntity[] result;

            try
            {
                result = entities as TEntity[] ?? entities.ToArray();
                await handler.GetCreateHandlerAsync(ref result, comparer);
                await context.Set<TEntity>().AddRangeAsync(result);
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.Create, NotifyType.Error, info, exception);
                return (exception.Message, null);
            }

            SetInfo(ActionType.Create, NotifyType.Success, info);
            return (null, result);
        }

        public async Task<(string? error, TEntity? result)> UpdateAsync(TEntity entity, string info)
        {
            try
            {
                await handler.GetUpdateHandlerAsync(ref entity);
                context.Set<TEntity>().Update(entity);
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.Update, NotifyType.Error, info, exception);
                return (exception.Message, null);
            }

            SetInfo(ActionType.Update, NotifyType.Success, info);
            return (null, entity);
        }
        public async Task<(string? error, TEntity[]? result)> UpdateAsync(IEnumerable<TEntity> entities, string info)
        {
            TEntity[] result;

            try
            {
                result = entities as TEntity[] ?? entities.ToArray();

                await handler.GetUpdateHandlerAsync(ref result);
                context.Set<TEntity>().UpdateRange(result);
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.Update, NotifyType.Error, info, exception);
                return (exception.Message, null);
            }

            SetInfo(ActionType.Update, NotifyType.Success, info);
            return (null, result);
        }

        public async Task<(string? error, TEntity? result)> CreateUpdateAsync(TEntity entity, string info)
        {
            try
            {
                await handler.GetCreateHandlerAsync(ref entity);
                await context.Set<TEntity>().AddAsync(entity);
                await context.SaveChangesAsync();
                SetInfo(ActionType.Create, NotifyType.Success, info);
            }
            catch (Exception exception)
            {
                try
                {
                    await handler.GetUpdateHandlerAsync(ref entity);
                    context.Set<TEntity>().Update(entity);
                    await context.SaveChangesAsync();
                    SetInfo(ActionType.Update, NotifyType.Success, info);
                }
                catch (Exception updateException)
                {
                    throw new DataException(updateException.Message);
                }

                SetInfo(ActionType.CreateUpdate, NotifyType.Error, info, exception);
                return (exception.Message, null);
            }

            return (null, entity);
        }
        public async Task<(string? error, TEntity[]? result)> CreateUpdateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            TEntity[] result;

            try
            {
                result = entities as TEntity[] ?? entities.ToArray();

                var createResult = result.ToArray();
                await handler.GetCreateHandlerAsync(ref createResult, comparer);
                await context.Set<TEntity>().AddRangeAsync(createResult);

                var updateResult = result.Except(createResult, comparer).ToArray();
                await handler.GetUpdateHandlerAsync(ref updateResult);
                context.Set<TEntity>().UpdateRange(updateResult);

                await context.SaveChangesAsync();

                result = createResult.Concat(updateResult).ToArray();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.CreateUpdate, NotifyType.Error, info, exception);
                return (exception.Message, null);
            }

            SetInfo(ActionType.CreateUpdate, NotifyType.Success, info);
            return (null, result);
        }

        public async Task<(string? error, TEntity[]? result)> CreateUpdateDeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            TEntity[] result;

            try
            {
                result = entities as TEntity[] ?? entities.ToArray();

                var createResult = result.ToArray();
                await handler.GetCreateHandlerAsync(ref createResult, comparer);
                await context.Set<TEntity>().AddRangeAsync(createResult);

                var updateResult = result.Except(createResult, comparer).ToArray();
                await handler.GetUpdateHandlerAsync(ref updateResult);
                context.Set<TEntity>().UpdateRange(updateResult);

                var deleteResult = context.Set<TEntity>().Except(createResult.Concat(updateResult), comparer);
                context.Set<TEntity>().RemoveRange(deleteResult);
                
                await context.SaveChangesAsync();

                result = createResult.Concat(updateResult).ToArray();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.CreateUpdate, NotifyType.Error, info, exception);
                return (exception.Message, null);
            }

            SetInfo(ActionType.CreateUpdate, NotifyType.Success, info);
            return (null, result);
        }
        public async Task<string?> ReCreateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            try
            {
                context.Set<TEntity>().RemoveRange(context.Set<TEntity>());

                var result = entities as TEntity[] ?? entities.ToArray();
                await handler.GetCreateHandlerAsync(ref result, comparer);
                await context.Set<TEntity>().AddRangeAsync(result);
                
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.ReCreate, NotifyType.Error, info, exception);
                return exception.Message;
            }

            SetInfo(ActionType.ReCreate, NotifyType.Success, info);
            return null;
        }

        public async Task<string?> DeleteAsync(string info)
        {
            try
            {
                context.Set<TEntity>().RemoveRange(context.Set<TEntity>());
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.Delete, NotifyType.Error, info, exception);
                return exception.Message;
            }

            SetInfo(ActionType.Delete, NotifyType.Success, info);
            return null;
        }
        public async Task<(string? error, TEntity? entity)> DeleteAsync(string info, params object[] id)
        {
            TEntity? entity;
            try
            {
                entity = await context.Set<TEntity>().FindAsync(id);
                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.Delete, NotifyType.Error, info, exception);
                return (exception.Message, null);
            }

            SetInfo(ActionType.Delete, NotifyType.Success, info);
            return (null, entity);
        }
        public async Task<string?> DeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            try
            {
                var result = entities as TEntity[] ?? entities.ToArray();
                var deleteResult = context.Set<TEntity>().Except(result, comparer);
                context.Set<TEntity>().RemoveRange(deleteResult);
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                SetInfo(ActionType.Delete, NotifyType.Error, info, exception);
                return exception.Message;
            }

            SetInfo(ActionType.Delete, NotifyType.Success, info);
            return null;
        }

        private static void SetInfo(ActionType actionType, NotifyType notifyType, string info, Exception? exception = null)
        {
            string action = actionType switch
            {
                ActionType.Create => string.Intern("Creating"),
                ActionType.Update => string.Intern("Updating"),
                ActionType.CreateUpdate => string.Intern("Creating and updating"),
                ActionType.ReCreate => string.Intern("Recreating"),
                _ => string.Intern("Deleting")
            };
            string notify = notifyType switch
            {
                NotifyType.Success => string.Intern("is success"),
                NotifyType.Error => string.Intern("is error"),
                _ => info
            };

            StringBuilder message = new(info.Length + 35);

            message.Append(action);
            message.Append(' ');
            message.Append(notify);
            message.Append(" for ");
            message.Append('\'');
            message.Append(info);
            message.Append('\'');

            Console.ForegroundColor = notifyType switch
            {
                NotifyType.Success => ConsoleColor.Green,
                NotifyType.Error => ConsoleColor.Red,
                _ => ConsoleColor.Cyan
            };

            Console.WriteLine(message.ToString());

            if (exception is not null)
                Console.WriteLine(exception.InnerException?.Message ?? exception.Message);

            Console.ForegroundColor = ConsoleColor.Gray;
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

    internal enum ActionType
    {
        Create,
        Update,
        Delete,
        CreateUpdate,
        ReCreate
    }
    internal enum NotifyType
    {
        Success,
        Error
    }
}
