using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task<TEntity?> CreateAsync(TEntity entity, string info)
        {
            if (handler.TryCheckEntity(entity, out TEntity? checkedEntity))
            {
                if (handler.GetIntersectedContextEntity(checkedEntity!) is null)
                {
                    await context.Set<TEntity>().AddAsync(checkedEntity!);
                    return await SaveAsync(info, RepositoryActionType.create) ? entity : null;
                }
                else
                    Console.WriteLine($"'{info}' is already!");
            }
            else
                Console.WriteLine($"created check failed for '{info}'!");

            return null;
        }
        public async Task<TEntity[]> CreateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
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
                        Console.WriteLine($"Is already entities count: {intersected.Length}!");
                        return await SaveAsync(info, RepositoryActionType.create) ? toAdd.ToArray() : Array.Empty<TEntity>();
                    }
                }

                await context.Set<TEntity>().AddRangeAsync(checkedEntities);
                return await SaveAsync(info, RepositoryActionType.create) ? checkedEntities.ToArray() : Array.Empty<TEntity>();
            }

            Console.WriteLine($"created check failed for '{info}'!");
            return Array.Empty<TEntity>();
        }
        public async Task<TEntity?> UpdateAsync(TEntity entity, string info)
        {
            if (handler.TryCheckEntity(entity, out TEntity checkedEntity))
            {
                var intersectedEntity = handler.GetIntersectedContextEntity(checkedEntity);

                if (intersectedEntity is not null)
                {
                    context.Entry(intersectedEntity).State = EntityState.Detached;
                    context.Set<TEntity>().Update(checkedEntity);
                    return await SaveAsync(info, RepositoryActionType.update) ? entity : null;
                }
                else
                    Console.WriteLine($"updated '{info}' not found!");
            }
            else
                Console.WriteLine($"updated check failed for '{info}'!");

            return null;
        }
        public async Task<TEntity[]> UpdateAsync(IEnumerable<TEntity> entities, string info)
        {
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

                    return await SaveAsync(info, RepositoryActionType.update) ? intersected : Array.Empty<TEntity>();
                }
                else
                    Console.WriteLine($"updated '{info}' not found!");
            }
            else
                Console.WriteLine($"updated check failed for '{info}'!");

            return Array.Empty<TEntity>();
        }
        public async Task<bool> CreateOrUpdateAsync(TEntity entity, string info)
        {
            if (handler.TryCheckEntity(entity, out TEntity? checkedEntity))
            {
                RepositoryActionType actionType = RepositoryActionType.create;

                var intersectedEntity = handler.GetIntersectedContextEntity(checkedEntity!);

                if (intersectedEntity is not null)
                {
                    context.Entry(intersectedEntity).State = EntityState.Detached;
                    actionType = RepositoryActionType.update;
                    context.Set<TEntity>().Update(entity);
                }
                else
                    await context.Set<TEntity>().AddAsync(entity);

                return await SaveAsync(info, actionType);
            }
            else
                Console.WriteLine($"created or updated check failed for '{info}'!");

            return false;
        }
        public async Task<bool> CreateOrUpdateAsync(IEnumerable<TEntity> entities, IEqualityComparer<TEntity> comparer, string info)
        {
            var addResult = false;
            var updateResult = false;

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
                        addResult = await SaveAsync(info, RepositoryActionType.create);
                    }

                    var newResult = entities.Except(toAdd, comparer).ToArray();

                    for (int i = 0; i < intersected.Length; i++)
                        for (int j = 0; j < newResult.Length; j++)
                            if (handler.UpdateEntity(intersected[i], newResult[j]))
                                break;

                    updateResult = await SaveAsync(info, RepositoryActionType.update);
                }
                else
                {
                    await context.Set<TEntity>().AddRangeAsync(entities);
                    addResult = await SaveAsync(info, RepositoryActionType.create);
                }
            }
            else
                Console.WriteLine($"created or updated check failed for '{info}'!");

            return addResult | updateResult;
        }
        public async Task<bool> DeleteAsync<TId>(TId id, string info)
        {
            var ctxEntity = await context.Set<TEntity>().FindAsync(id);

            if (ctxEntity is null)
            {
                Console.WriteLine($"'{info}' to delete not found");
                return true;
            }

            context.Set<TEntity>().Remove(ctxEntity);

            return await SaveAsync(info, RepositoryActionType.delete);
        }
        public async Task<bool> DeleteAsync(IEnumerable<TEntity> entities, string info)
        {
            context.Set<TEntity>().RemoveRange(entities);
            return await SaveAsync(info, RepositoryActionType.delete);
        }

        public async Task<TEntity[]> FindAsync(Expression<Func<TEntity, bool>> predicate) => await context.Set<TEntity>().AsNoTracking().Where(predicate).ToArrayAsync();

        async Task<bool> SaveAsync(string info, RepositoryActionType actionType)
        {
            string actionName = actionType switch
            {
                RepositoryActionType.create => "created",
                RepositoryActionType.update => "updated",
                _ => "deleted",
            };
            int result = -1;
            string savingError = "Saving error: ";
            try
            {
                result = await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                savingError += ex.InnerException?.Message ?? ex.Message;
            }

            if (result >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"'{info}' has been {actionName}.");
                Console.ForegroundColor = ConsoleColor.Gray;
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"'{info}' has not been {actionName}! \n{savingError}");
                Console.ForegroundColor = ConsoleColor.Gray;
                return false;
            }
        }
    }
    enum RepositoryActionType
    {
        create,
        update,
        delete,
    }
}
