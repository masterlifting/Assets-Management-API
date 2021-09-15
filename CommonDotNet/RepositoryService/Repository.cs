using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CommonServices.Models.Dto.Http;

namespace CommonServices.RepositoryService
{
    public class Repository<TEntity, TContext> where TEntity : class where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IRepository<TEntity> handler;

        protected Repository(TContext context, IRepository<TEntity> handler)
        {
            this.context = context;
            this.handler = handler;
        }

        public async Task<(string[] errors, TEntity? result)> CreateAsync(TEntity entity, string info)
        {
            var errors = Array.Empty<string>();

            var (trySuccess, checkedEntity) = await handler.TryCheckEntityAsync(entity);

            if (trySuccess)
            {
                if (await handler.GetAlreadyEntityAsync(checkedEntity!) is null)
                {
                    await context.Set<TEntity>().AddAsync(checkedEntity!);
                    errors = await SaveAsync(info, ActionType.Create);
                    return errors.Any() ? (errors, null) : (errors, checkedEntity);
                }

                SetInfo(ActionType.Create, NotifyType.Already, info, ref errors);
            }
            else
                SetInfo(ActionType.Create, NotifyType.CheckFailed, info, ref errors);

            return (errors, null);
        }
        public async Task<(string[] errors, TEntity[] result)> CreateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            var errors = Array.Empty<string>();

            var (trySuccess, checkedEntities) = await handler.TryCheckEntitiesAsync(entities);

            if (trySuccess)
            {
                var intersectedEntities = handler.GetAlreadyEntitiesQuery(checkedEntities);

                if (intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    var toAdd = checkedEntities.Except(intersected, comparer).ToArray();

                    if (toAdd.Any())
                    {
                        await context.Set<TEntity>().AddRangeAsync(toAdd);

                        SetInfo(ActionType.Create, NotifyType.Info, info, ref errors, $" is already count: {intersected.Length}");
                        errors = await SaveAsync(info, ActionType.Create);
                        return errors.Any() ? (errors, Array.Empty<TEntity>()) : (errors, toAdd);
                    }
                }

                await context.Set<TEntity>().AddRangeAsync(checkedEntities);
                errors = await SaveAsync(info, ActionType.Create);
                return errors.Any() ? (errors, Array.Empty<TEntity>()) : (errors, checkedEntities);
            }

            SetInfo(ActionType.Create, NotifyType.CheckFailed, info, ref errors);
            return (errors, Array.Empty<TEntity>());
        }
        public async Task<(string[] errors, TEntity? result)> UpdateAsync(TEntity entity, string info)
        {
            var errors = Array.Empty<string>();

            var (trySuccess, checkedEntity) = await handler.TryCheckEntityAsync(entity);

            if (trySuccess)
            {
                var intersectedEntity = await handler.GetAlreadyEntityAsync(checkedEntity!);

                if (intersectedEntity is not null)
                {
                    context.Entry(intersectedEntity).State = EntityState.Detached;
                    context.Set<TEntity>().Update(checkedEntity!);
                    errors = await SaveAsync(info, ActionType.Update);
                    return errors.Any() ? (errors, null) : (errors, checkedEntity);
                }

                SetInfo(ActionType.Update, NotifyType.NotFound, info, ref errors);
            }
            else
                SetInfo(ActionType.Update, NotifyType.CheckFailed, info, ref errors);

            return (errors, null);
        }
        public async Task<(string[] errors, TEntity[] result)> UpdateAsync(IEnumerable<TEntity> entities, string info)
        {
            var errors = Array.Empty<string>();

            var (trySuccess, checkedEntities) = await handler.TryCheckEntitiesAsync(entities);

            if (trySuccess)
            {
                var intersectedEntities = handler.GetAlreadyEntitiesQuery(checkedEntities);

                if (intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    foreach (var contextEntity in intersected)
                        foreach (var newEntity in checkedEntities)
                            if (handler.IsUpdate(contextEntity, newEntity))
                                break;

                    errors = await SaveAsync(info, ActionType.Update);
                    return errors.Any() ? (errors, Array.Empty<TEntity>()) : (errors, intersected);
                }

                SetInfo(ActionType.Update, NotifyType.NotFound, info, ref errors);
            }
            else
                SetInfo(ActionType.Update, NotifyType.CheckFailed, info, ref errors);

            return (errors, Array.Empty<TEntity>());
        }
        public async Task<string[]> CreateUpdateAsync(TEntity entity, string info)
        {
            var errors = Array.Empty<string>();

            var (trySuccess, checkedEntity) = await handler.TryCheckEntityAsync(entity);

            if (trySuccess)
            {
                var actionType = ActionType.Create;

                var intersectedEntity = await handler.GetAlreadyEntityAsync(checkedEntity!);

                if (intersectedEntity is not null)
                {
                    context.Entry(intersectedEntity).State = EntityState.Detached;
                    actionType = ActionType.Update;
                    context.Set<TEntity>().Update(entity);
                }
                else
                    await context.Set<TEntity>().AddAsync(entity);

                return await SaveAsync(info, actionType);
            }

            SetInfo(ActionType.CreateUpdate, NotifyType.CheckFailed, info, ref errors);

            return errors;
        }
        public async Task<string[]> CreateUpdateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            var errors = Array.Empty<string>();

            var arrayEntities = entities.ToArray();

            var (trySuccess, checkedEntities) = await handler.TryCheckEntitiesAsync(arrayEntities);

            if (trySuccess)
            {
                var intersectedEntities = handler.GetAlreadyEntitiesQuery(checkedEntities);

                if (intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    var toAdd = arrayEntities.Except(intersected, comparer).ToArray();

                    if (toAdd.Any())
                    {
                        await context.Set<TEntity>().AddRangeAsync(toAdd);
                        errors = await SaveAsync(info, ActionType.Create);
                    }

                    var toUpdate = arrayEntities.Except(toAdd, comparer).ToArray();

                    foreach (var contextEntity in intersected)
                        foreach (var newEntity in toUpdate)
                            if (handler.IsUpdate(contextEntity, newEntity))
                                break;

                    errors = errors.Concat(await SaveAsync(info, ActionType.Update)).ToArray();
                }
                else
                {
                    await context.Set<TEntity>().AddRangeAsync(arrayEntities);
                    errors = await SaveAsync(info, ActionType.Create);
                }
            }
            else
                SetInfo(ActionType.CreateUpdate, NotifyType.CheckFailed, info, ref errors);

            return errors;
        }
        public async Task<string[]> CreateUpdateDeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, Expression<Func<TEntity, bool>> deletePredicate, string info)
        {
            var errors = Array.Empty<string>();

            var all = await context.Set<TEntity>().Where(deletePredicate).ToArrayAsync();

            var arrayEntities = entities.ToArray();

            var (trySuccess, checkedEntities) = await handler.TryCheckEntitiesAsync(arrayEntities);

            if (trySuccess)
            {
                var intersectedEntities = handler.GetAlreadyEntitiesQuery(checkedEntities);

                if (intersectedEntities.Any())
                {
                    var toUpdate = await intersectedEntities.ToArrayAsync();
                    var toAdd = arrayEntities.Except(toUpdate, comparer).ToArray();

                    if (toAdd.Any())
                    {
                        await context.Set<TEntity>().AddRangeAsync(toAdd);
                        errors = await SaveAsync(info, ActionType.Create);
                    }

                    var newUpdateResult = arrayEntities.Except(toAdd, comparer).ToArray();

                    foreach (var oldEntity in toUpdate)
                        foreach (var newEntity in newUpdateResult)
                            if (handler.IsUpdate(oldEntity, newEntity))
                                break;

                    errors = errors.Concat(await SaveAsync(info, ActionType.Update)).ToArray();

                    var toDelete = all.Except(toAdd.Union(toUpdate), comparer).ToArray();

                    // ReSharper disable once InvertIf
                    if (toDelete.Any())
                    {
                        context.Set<TEntity>().RemoveRange(toDelete);
                        errors = errors.Concat(await SaveAsync(info, ActionType.Delete)).ToArray();
                    }
                }
                else
                {
                    await context.Set<TEntity>().AddRangeAsync(arrayEntities);
                    errors = await SaveAsync(info, ActionType.Create);

                    var toDelete = all.Except(arrayEntities, comparer).ToArray();
                    // ReSharper disable once InvertIf
                    if (toDelete.Any())
                    {
                        context.Set<TEntity>().RemoveRange(toDelete);
                        errors = errors.Concat(await SaveAsync(info, ActionType.Delete)).ToArray();
                    }
                }
            }
            else
                SetInfo(ActionType.CreateUpdateDelete, NotifyType.CheckFailed, info, ref errors);

            return errors;
        }
        public async Task<string[]> DeleteAsync<TId>(TId id, string info)
        {
            var errors = Array.Empty<string>();

            var ctxEntity = await context.Set<TEntity>().FindAsync(id);

            if (ctxEntity is null)
            {
                SetInfo(ActionType.Delete, NotifyType.NotFound, info, ref errors);
                return errors;
            }

            context.Set<TEntity>().Remove(ctxEntity);

            return await SaveAsync(info, ActionType.Delete);
        }
        public async Task<string[]> DeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            var errors = Array.Empty<string>();

            var arrayEntities = entities.ToArray();

            var (trySuccess, checkedEntities) = await handler.TryCheckEntitiesAsync(arrayEntities);

            if (trySuccess)
            {
                var intersectedEntities = handler.GetAlreadyEntitiesQuery(checkedEntities);

                if (intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    if (intersected.Length != arrayEntities.Length)
                    {
                        var notDeleted = arrayEntities.Except(intersected, comparer);
                        SetInfo(ActionType.Delete, NotifyType.Info, info, ref errors, $" not found count: {notDeleted.Count()}");
                    }

                    context.Set<TEntity>().RemoveRange(intersected);
                    return await SaveAsync(info, ActionType.Delete);
                }

                SetInfo(ActionType.Delete, NotifyType.NotFound, info, ref errors, $" count: {arrayEntities.Length}");
            }
            else
                SetInfo(ActionType.Delete, NotifyType.CheckFailed, info, ref errors);

            return errors;
        }

        public async Task DeleteAsync(string info)
        {
            context.Set<TEntity>().RemoveRange(context.Set<TEntity>());

            await SaveAsync(info, ActionType.Delete);
        }

        private async Task<string[]> SaveAsync(string info, ActionType actionType)
        {
            var errors = Array.Empty<string>();
            try
            {
                if (await context.SaveChangesAsync() >= 0)
                    SetInfo(actionType, NotifyType.Success, info, ref errors);
            }
            catch (Exception ex)
            {
                SetInfo(actionType, NotifyType.SavingFailed, info, ref errors);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            return errors;
        }
        private static void SetInfo(ActionType actionType, NotifyType notifyType, string info, ref string[] errors, string msg = "")
        {
            string action = actionType switch
            {
                ActionType.Create => string.Intern("creating"),
                ActionType.Update => string.Intern("updating"),
                ActionType.CreateUpdate => string.Intern("creating/updating"),
                ActionType.CreateUpdateDelete => string.Intern("creating/updating/deleting"),
                _ => string.Intern("deleting")
            };
            string notify = notifyType switch
            {
                NotifyType.Success => string.Intern("success"),
                NotifyType.Already => string.Intern("is already"),
                NotifyType.NotFound => string.Intern("not found"),
                NotifyType.CheckFailed => string.Intern("check failed"),
                NotifyType.SavingFailed => string.Intern("save failed"),
                NotifyType.Info => msg,
                _ => throw new NotImplementedException()
            };

            StringBuilder builder = new(info.Length + 35);
            builder.Append(action);

            if (notifyType != NotifyType.Info)
                builder.Append(msg);

            builder.Append(' ');
            builder.Append(notify);
            builder.Append(" for ");
            builder.Append('\'');
            builder.Append(info);
            builder.Append('\'');

            string message = builder.ToString();

            if (notifyType == NotifyType.Success)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (notifyType == NotifyType.Info)
                Console.ForegroundColor = ConsoleColor.Cyan;
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                errors = errors.Append(message).ToArray();
            }

            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public DbSet<QEntity> GetDbSetBy<QEntity>() where QEntity : class => context.Set<QEntity>();
        public IQueryable<TEntity> QueryFilter(Expression<Func<TEntity, bool>> predicate) => context.Set<TEntity>().Where(predicate);

        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().CountAsync(predicate);
        public async Task<int> GetCountAsync() => await context.Set<TEntity>().AsNoTracking().CountAsync();

        public async Task<TEntity[]> FindAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().Where(predicate).ToArrayAsync();
        public async Task<TEntity?> FindAsync(params object[] key) => await context.Set<TEntity>().FindAsync(key);

        public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, TResult>> selector) =>
            await context.Set<TEntity>().Select(selector).ToArrayAsync();
        public async Task<TResult[]> GetSampleAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector) =>
            await context.Set<TEntity>().Where(predicate).Select(selector).ToArrayAsync();

        public IQueryable<TEntity> QueryPaginatedResult(PaginationRequestModel pagination) =>
            context.Set<TEntity>()
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit);
        public IQueryable<TEntity> QueryPaginatedResult<TSelector>(PaginationRequestModel pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
            context.Set<TEntity>()
                .OrderBy(orderSelector)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit);
        public IQueryable<TEntity> QueryPaginatedResult<TSelector1, TSelector2>(PaginationRequestModel pagination, Expression<Func<TEntity, TSelector1>> orderSelector1, Expression<Func<TEntity, TSelector2>> orderSelector2) =>
            context.Set<TEntity>()
                .OrderBy(orderSelector1)
                .ThenBy(orderSelector2)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit);
        public IQueryable<TEntity> QueryPaginatedResult(IQueryable<TEntity> query, PaginationRequestModel pagination) =>
           query.Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);
        public IQueryable<TEntity> QueryPaginatedResult<TSelector>(IQueryable<TEntity> query, PaginationRequestModel pagination, Expression<Func<TEntity, TSelector>> orderSelector) =>
            query.OrderBy(orderSelector).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);
        public IQueryable<TEntity> QueryPaginatedResult<TSelector1, TSelector2>(IQueryable<TEntity> query, PaginationRequestModel pagination, Expression<Func<TEntity, TSelector1>> orderSelector1, Expression<Func<TEntity, TSelector2>> orderSelector2) =>
            query.OrderBy(orderSelector1).ThenBy(orderSelector2).Skip((pagination.Page - 1) * pagination.Limit).Take(pagination.Limit);
    }

    internal enum ActionType
    {
        Create,
        Update,
        Delete,
        CreateUpdate,
        CreateUpdateDelete
    }
    internal enum NotifyType
    {
        Success,
        Info,
        Already,
        NotFound,
        CheckFailed,
        SavingFailed
    }
}
