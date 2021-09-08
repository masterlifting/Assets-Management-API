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

        public Repository(TContext context, IRepository<TEntity> handler)
        {
            this.context = context;
            this.handler = handler;
        }

        public async Task<(string[] errors, TEntity? result)> CreateAsync(TEntity entity, string info)
        {
            var _errors = Array.Empty<string>();

            if (handler.TryCheckEntity(entity, out TEntity? checkedEntity))
            {
                if (handler.GetIntersectedContextEntity(checkedEntity!) is null)
                {
                    await context.Set<TEntity>().AddAsync(checkedEntity!);
                    _errors = await SaveAsync(info, ActionType.create);
                    return _errors.Any() ? (_errors, null) : (_errors, checkedEntity);
                }
                else
                    SetInfo(ActionType.create, NotifyType.already, info, ref _errors);
            }
            else
                SetInfo(ActionType.create, NotifyType.check_failed, info, ref _errors);

            return (_errors, null);
        }
        public async Task<(string[] errors, TEntity[] result)> CreateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            var _errors = Array.Empty<string>();

            if (handler.TryCheckEntities(entities, out TEntity[] checkedEntities))
            {
                var intersectedEntities = handler.GetIntersectedContextEntities(checkedEntities);

                if (intersectedEntities is not null && intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    var toAdd = checkedEntities.Except(intersected, comparer);

                    if (toAdd.Any())
                    {
                        await context.Set<TEntity>().AddRangeAsync(toAdd);

                        SetInfo(ActionType.create, NotifyType.info, info, ref _errors, $" is already count: {intersected.Length}");
                        _errors = await SaveAsync(info, ActionType.create);
                        return _errors.Any() ? (_errors, Array.Empty<TEntity>()) : (_errors, toAdd.ToArray());
                    }
                }

                await context.Set<TEntity>().AddRangeAsync(checkedEntities);
                _errors = await SaveAsync(info, ActionType.create);
                return _errors.Any() ? (_errors, Array.Empty<TEntity>()) : (_errors, checkedEntities.ToArray());
            }

            SetInfo(ActionType.create, NotifyType.check_failed, info, ref _errors);
            return (_errors, Array.Empty<TEntity>());
        }
        public async Task<(string[] errors, TEntity? result)> UpdateAsync(TEntity entity, string info)
        {
            var _errors = Array.Empty<string>();

            if (handler.TryCheckEntity(entity, out TEntity? checkedEntity))
            {
                var intersectedEntity = handler.GetIntersectedContextEntity(checkedEntity!);

                if (intersectedEntity is not null)
                {
                    context.Entry(intersectedEntity).State = EntityState.Detached;
                    context.Set<TEntity>().Update(checkedEntity!);
                    _errors = await SaveAsync(info, ActionType.update);
                    return _errors.Any() ? (_errors, null) : (_errors, checkedEntity);
                }
                else
                    SetInfo(ActionType.update, NotifyType.not_found, info, ref _errors);
            }
            else
                SetInfo(ActionType.update, NotifyType.check_failed, info, ref _errors);

            return (_errors, null);
        }
        public async Task<(string[] errors, TEntity[] result)> UpdateAsync(IEnumerable<TEntity> entities, string info)
        {
            var _errors = Array.Empty<string>();

            if (handler.TryCheckEntities(entities, out TEntity[] checkedEntities))
            {
                var intersectedEntities = handler.GetIntersectedContextEntities(checkedEntities);

                if (intersectedEntities is not null && intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    for (int i = 0; i < intersected.Length; i++)
                        for (int j = 0; j < checkedEntities.Length; j++)
                            if (handler.UpdateEntity(intersected[i], checkedEntities[j]))
                                break;

                    _errors = await SaveAsync(info, ActionType.update);
                    return _errors.Any() ? (_errors, Array.Empty<TEntity>()) : (_errors, intersected);
                }
                else
                    SetInfo(ActionType.update, NotifyType.not_found, info, ref _errors);
            }
            else
                SetInfo(ActionType.update, NotifyType.check_failed, info, ref _errors);

            return (_errors, Array.Empty<TEntity>());
        }
        public async Task<string[]> CreateUpdateAsync(TEntity entity, string info)
        {
            var _errors = Array.Empty<string>();

            if (handler.TryCheckEntity(entity, out TEntity? checkedEntity))
            {
                ActionType actionType = ActionType.create;

                var intersectedEntity = handler.GetIntersectedContextEntity(checkedEntity!);

                if (intersectedEntity is not null)
                {
                    context.Entry(intersectedEntity).State = EntityState.Detached;
                    actionType = ActionType.update;
                    context.Set<TEntity>().Update(entity);
                }
                else
                    await context.Set<TEntity>().AddAsync(entity);

                return await SaveAsync(info, actionType);
            }
            else
                SetInfo(ActionType.create_update, NotifyType.check_failed, info, ref _errors);

            return _errors;
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
                        _errors = await SaveAsync(info, ActionType.create);
                    }

                    var newResult = entities.Except(toAdd, comparer).ToArray();

                    for (int i = 0; i < intersected.Length; i++)
                        for (int j = 0; j < newResult.Length; j++)
                            if (handler.UpdateEntity(intersected[i], newResult[j]))
                                break;

                    _errors = _errors.Concat(await SaveAsync(info, ActionType.update)).ToArray();
                }
                else
                {
                    await context.Set<TEntity>().AddRangeAsync(entities);
                    _errors = await SaveAsync(info, ActionType.create);
                }
            }
            else
                SetInfo(ActionType.create_update, NotifyType.check_failed, info, ref _errors);

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
                        _errors = await SaveAsync(info, ActionType.create);
                    }

                    var newUpdateResult = entities.Except(toAdd, comparer).ToArray();

                    for (int i = 0; i < toUpdate.Length; i++)
                        for (int j = 0; j < newUpdateResult.Length; j++)
                            if (handler.UpdateEntity(toUpdate[i], newUpdateResult[j]))
                                break;

                    _errors = _errors.Concat(await SaveAsync(info, ActionType.update)).ToArray();

                    var toDelete = all.Except(toAdd.Union(toUpdate), comparer);
                    if (toDelete.Any())
                    {
                        context.Set<TEntity>().RemoveRange(toDelete);
                        _errors = _errors.Concat(await SaveAsync(info, ActionType.delete)).ToArray();
                    }
                }
                else
                {
                    await context.Set<TEntity>().AddRangeAsync(entities);
                    _errors = await SaveAsync(info, ActionType.create);

                    var toDelete = all.Except(entities, comparer);
                    if (toDelete.Any())
                    {
                        context.Set<TEntity>().RemoveRange(toDelete);
                        _errors = _errors.Concat(await SaveAsync(info, ActionType.delete)).ToArray();
                    }
                }
            }
            else
                SetInfo(ActionType.create_update_delete, NotifyType.check_failed, info, ref _errors);

            return _errors;
        }
        public async Task<string[]> DeleteAsync<TId>(TId id, string info)
        {
            var _errors = Array.Empty<string>();

            var ctxEntity = await context.Set<TEntity>().FindAsync(id);

            if (ctxEntity is null)
            {
                SetInfo(ActionType.delete, NotifyType.not_found, info, ref _errors);
                return _errors;
            }

            context.Set<TEntity>().Remove(ctxEntity);

            return await SaveAsync(info, ActionType.delete);
        }
        public async Task<string[]> DeleteAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            var _errors = Array.Empty<string>();

            if (handler.TryCheckEntities(entities, out TEntity[] checkedEntities))
            {
                var intersectedEntities = handler.GetIntersectedContextEntities(checkedEntities);

                if (intersectedEntities is not null && intersectedEntities.Any())
                {
                    var intersected = await intersectedEntities.ToArrayAsync();

                    if (intersected.Length != entities.Count())
                    {
                        var notDeleted = entities.Except(intersected, comparer);
                        SetInfo(ActionType.delete, NotifyType.info, info, ref _errors, $" not found count: {notDeleted.Count()}");
                    }

                    context.Set<TEntity>().RemoveRange(intersected);
                    return await SaveAsync(info, ActionType.delete);
                }
                else
                    SetInfo(ActionType.delete, NotifyType.not_found, info, ref _errors, $" count: {entities.Count()}");
            }
            else
                SetInfo(ActionType.delete, NotifyType.check_failed, info, ref _errors);

            return _errors;
        }

        public async Task<TEntity[]> FindAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().Where(predicate).ToArrayAsync();

        async Task<string[]> SaveAsync(string info, ActionType actionType)
        {
            var _errors = Array.Empty<string>();
            try
            {
                if (await context.SaveChangesAsync() >= 0)
                    SetInfo(actionType, NotifyType.success, info, ref _errors);
            }
            catch (Exception ex)
            {
                SetInfo(actionType, NotifyType.saving_failed, info, ref _errors);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            return _errors;
        }
        static void SetInfo(ActionType actionType, NotifyType notifyType, string info, ref string[] errors, string msg = "")
        {
            string action = actionType switch
            {
                ActionType.create => string.Intern("creating") + msg,
                ActionType.update => string.Intern("updating") + msg,
                ActionType.create_update => string.Intern("creating/updating") + msg,
                ActionType.create_update_delete => string.Intern("creating/updating/deliting")  + msg,
                _ => string.Intern("deleting") + msg
            };
            string notify = notifyType switch
            {
                NotifyType.success => string.Intern("success"),
                NotifyType.already => string.Intern("is already"),
                NotifyType.not_found => string.Intern("not found"),
                NotifyType.check_failed => string.Intern("check failed"),
                NotifyType.saving_failed => string.Intern("saving failed"),
                NotifyType.info => msg,
                _ => throw new NotImplementedException()
            };

            StringBuilder builder = new(info.Length + 35);
            builder.Append(action);
            builder.Append(' ');
            builder.Append(notify);
            builder.Append(" for ");
            builder.Append('\'');
            builder.Append(info);
            builder.Append('\'');
            builder.Append('!');

            string message = builder.ToString();

            if (notifyType == NotifyType.success)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (notifyType == NotifyType.info)
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
    enum ActionType
    {
        create,
        update,
        delete,
        create_update,
        create_update_delete
    }
    enum NotifyType
    {
        success,
        info,
        already,
        not_found,
        check_failed,
        saving_failed
    }
}
