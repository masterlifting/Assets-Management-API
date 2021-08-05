using System;
using System.Text.Json;
using System.Threading.Tasks;

using static IM.Services.Companies.Prices.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Prices.Api.DataAccess.Repository
{
    public class EntityRepository<TEntity, TId> where TEntity : class
    {
        private readonly PricesContext context;
        private readonly IEntityChecker<TEntity> checker;

        public EntityRepository(PricesContext context, IEntityChecker<TEntity> checker)
        {
            this.context = context;
            this.checker = checker;
        }

        public async Task<bool> AddAsync(TEntity entity, string info)
        {
            if (checker.WithError(entity))
                return false;

            if (await checker.IsAlreadyAsync(entity))
                return true;

            await context.Set<TEntity>().AddAsync(entity);

            return await SaveAsync(info, RepositoryActionType.create);
        }
        public async Task<bool> EditAsync(TId entityId, TEntity entity, string info)
        {
            if (checker.WithError(entity))
                return false;

            var ctxEntity = await context.Set<TEntity>().FindAsync(entityId);
            
            if(ctxEntity is null)
            {
                Console.WriteLine($"data to edit not found");
                return false;
            }

            ctxEntity = entity;

            context.Set<TEntity>().Update(ctxEntity);
            
            return await SaveAsync(info, RepositoryActionType.update);
        }
        public async Task<bool> RemoveAsync(TId entityId, string info)
        {
            var ctxEntity = await context.Set<TEntity>().FindAsync(entityId);

            if (ctxEntity is not null)
                context.Set<TEntity>().Remove(ctxEntity);

            return await SaveAsync(info, RepositoryActionType.delete);
        }

        public bool TrySerialize(string data, out TEntity entity)
        {
            entity = null;

            try
            {
                entity = JsonSerializer.Deserialize<TEntity>(data);
                return true;
            }
            catch (JsonException ex)
            {
                Console.WriteLine("unserializable! Exception: " + ex.Message);
                return false;
            }
        }
        async Task<bool> SaveAsync(string info, RepositoryActionType actionType)
        {
            string actionName = actionType switch
            {
                RepositoryActionType.create => "created",
                RepositoryActionType.update => "updated",
                _ => "deleted",
            };

            if (await context.SaveChangesAsync() > 0)
            {
                Console.WriteLine($"'{info}' has been {actionName}.");
                return true;
            }
            else
            {
                Console.WriteLine($"'{info}' has not been {actionName}! Saving error!");
                return false;
            }
        }
    }
}
