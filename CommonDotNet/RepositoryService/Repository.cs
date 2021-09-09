using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

            if (handler.TryCheckEntity(entity, out var checkedEntity))
            {
                if (handler.GetIntersectedContextEntity(checkedEntity!) is null)
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

            if (handler.TryCheckEntities(entities, out TEntity[] checkedEntities))
            {
                var intersectedEntities = handler.GetIntersectedContextEntities(checkedEntities);

                if (intersectedEntities is not null && intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    var toAdd = checkedEntities.Except(intersected, comparer).ToArray();

                    if (toAdd.Any())
                    {
                        await context.Set<TEntity>().AddRangeAsync(toAdd);

                        SetInfo(ActionType.Create, NotifyType.Info, info, ref errors, $" is already count: {intersected.Length}");
                        errors = await SaveAsync(info, ActionType.Create);
                        return errors.Any() ? (errors, Array.Empty<TEntity>()) : (errors, toAdd.ToArray());
                    }
                }

                await context.Set<TEntity>().AddRangeAsync(checkedEntities);
                errors = await SaveAsync(info, ActionType.Create);
                return errors.Any() ? (errors, Array.Empty<TEntity>()) : (errors, checkedEntities.ToArray());
            }

            SetInfo(ActionType.Create, NotifyType.CheckFailed, info, ref errors);
            return (errors, Array.Empty<TEntity>());
        }
        public async Task<(string[] errors, TEntity? result)> UpdateAsync(TEntity entity, string info)
        {
            var errors = Array.Empty<string>();

            if (handler.TryCheckEntity(entity, out var checkedEntity))
            {
                var intersectedEntity = handler.GetIntersectedContextEntity(checkedEntity!);

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

            if (handler.TryCheckEntities(entities, out TEntity[] checkedEntities))
            {
                var intersectedEntities = handler.GetIntersectedContextEntities(checkedEntities);

                if (intersectedEntities is not null && intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    foreach (var oldResult in intersected)
                        foreach (var newResult in checkedEntities)
                            if (handler.UpdateEntity(oldResult, newResult))
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

            if (handler.TryCheckEntity(entity, out var checkedEntity))
            {
                var actionType = ActionType.Create;

                var intersectedEntity = handler.GetIntersectedContextEntity(checkedEntity!);

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
            var _errors = Array.Empty<string>();

            if (handler.TryCheckEntities(entities, out TEntity[] checkedEntities))
            {
                var intersectedEntities = handler.GetIntersectedContextEntities(checkedEntities);

                if (intersectedEntities is not null && intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    var toAdd = entities.Except(intersected, comparer);

                    if (toAdd.Any())
                    {
                        await context.Set<TEntity>().AddRangeAsync(toAdd);
                        _errors = await SaveAsync(info, ActionType.Create);
                    }

                    var newResult = entities.Except(toAdd, comparer).ToArray();

                    for (int i = 0; i < intersected.Length; i++)
                        for (int j = 0; j < newResult.Length; j++)
                            if (handler.UpdateEntity(intersected[i], newResult[j]))
                                break;

                    _errors = _errors.Concat(await SaveAsync(info, ActionType.Update)).ToArray();
                }
                else
                {
                    await context.Set<TEntity>().AddRangeAsync(entities);
                    _errors = await SaveAsync(info, ActionType.Create);
                }
            }
            else
                SetInfo(ActionType.CreateUpdate, NotifyType.CheckFailed, info, ref _errors);

            return _errors;
        }
        public async Task<string[]> CreateUpdateDeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, Expression<Func<TEntity, bool>> deletePredicate, string info)
        {
            var _errors = Array.Empty<string>();

            var all = await context.Set<TEntity>().Where(deletePredicate).ToArrayAsync();

            if (handler.TryCheckEntities(entities, out TEntity[] checkedEntities))
            {
                var intersectedEntities = handler.GetIntersectedContextEntities(checkedEntities);

                if (intersectedEntities is not null && intersectedEntities.Any())
                {
                    var toUpdate = await intersectedEntities.ToArrayAsync();
                    var toAdd = entities.Except(toUpdate, comparer);

                    if (toAdd.Any())
                    {
                        await context.Set<TEntity>().AddRangeAsync(toAdd);
                        _errors = await SaveAsync(info, ActionType.Create);
                    }

                    var newUpdateResult = entities.Except(toAdd, comparer).ToArray();

                    for (int i = 0; i < toUpdate.Length; i++)
                        for (int j = 0; j < newUpdateResult.Length; j++)
                            if (handler.UpdateEntity(toUpdate[i], newUpdateResult[j]))
                                break;

                    _errors = _errors.Concat(await SaveAsync(info, ActionType.Update)).ToArray();

                    var toDelete = all.Except(toAdd.Union(toUpdate), comparer);
                    if (toDelete.Any())
                    {
                        context.Set<TEntity>().RemoveRange(toDelete);
                        _errors = _errors.Concat(await SaveAsync(info, ActionType.Delete)).ToArray();
                    }
                }
                else
                {
                    await context.Set<TEntity>().AddRangeAsync(entities);
                    _errors = await SaveAsync(info, ActionType.Create);

                    var toDelete = all.Except(entities, comparer);
                    if (toDelete.Any())
                    {
                        context.Set<TEntity>().RemoveRange(toDelete);
                        _errors = _errors.Concat(await SaveAsync(info, ActionType.Delete)).ToArray();
                    }
                }
            }
            else
                SetInfo(ActionType.CreateUpdateDelete, NotifyType.CheckFailed, info, ref _errors);

            return _errors;
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

            var enumerableEntities = entities as TEntity[] ?? entities.ToArray();

            if (handler.TryCheckEntities(enumerableEntities, out TEntity[] checkedEntities))
            {
                var intersectedEntities = handler.GetIntersectedContextEntities(checkedEntities);

                if (intersectedEntities is not null && intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    if (intersected.Length != enumerableEntities.Length)
                    {
                        var notDeleted = enumerableEntities.Except(intersected, comparer);
                        SetInfo(ActionType.Delete, NotifyType.Info, info, ref errors, $" not found count: {notDeleted.Count()}");
                    }

                    context.Set<TEntity>().RemoveRange(intersected);
                    return await SaveAsync(info, ActionType.Delete);
                }
                else
                    SetInfo(ActionType.Delete, NotifyType.NotFound, info, ref errors, $" count: {enumerableEntities.Length}");
            }
            else
                SetInfo(ActionType.Delete, NotifyType.CheckFailed, info, ref errors);

            return errors;
        }

        public async Task<TEntity[]> FindAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().Where(predicate).ToArrayAsync();

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
