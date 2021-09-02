using Microsoft.EntityFrameworkCore;

using System;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;

namespace CommonServices.RepositoryService
{
    public class EntityRepository<TEntity, TContext> where TEntity : class where TContext : DbContext
    {
        private readonly TContext context;
        private readonly IEntityChecker<TEntity> checker;

        public EntityRepository(TContext context, IEntityChecker<TEntity> checker)
        {
            this.context = context;
            this.checker = checker;
        }

        public async Task<bool> CreateAsync(TEntity entity, string info)
        {
            if (checker.WithError(entity))
                return false;

            if (await checker.IsAlreadyAsync(entity))
                return true;

            await context.Set<TEntity>().AddAsync(entity);

            return await SaveAsync(info, RepositoryActionType.create);
        }
        public async Task<bool> UpdateAsync(TEntity entity, string info)
        {
            if (checker.WithError(entity))
                return false;

            context.Set<TEntity>().Update(entity);

            return await SaveAsync(info, RepositoryActionType.update);
        }
        public async Task<bool> CreateOrUpdateAsync<TId>(TId id, TEntity entity, string info)
        {
            var ctxEntity = await context.Set<TEntity>().FindAsync(id);
            return ctxEntity is null ? await CreateAsync(entity, info) : await UpdateAsync(entity, info);
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
        async Task<bool> SaveAsync(string info, RepositoryActionType actionType)
        {
            string actionName = actionType switch
            {
                RepositoryActionType.create => "created",
                RepositoryActionType.update => "updated",
                _ => "deleted"
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
}
