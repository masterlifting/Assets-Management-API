using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using static IM.Services.Companies.Reports.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Reports.Api.DataAccess.Repository
{
    public class EntityRepository<TEntity, TId> where TEntity : class
    {
        private readonly ReportsContext context;
        private readonly IEntityChecker<TEntity> checker;

        public EntityRepository(ReportsContext context, IEntityChecker<TEntity> checker)
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
        public async Task<bool> UpdateAsync(TId entityId, TEntity entity, string info)
        {
            if (checker.WithError(entity))
                return false;

            var ctxEntity = await context.Set<TEntity>().FindAsync(entityId);

            if (ctxEntity is null)
            {
                Console.WriteLine("data to update not found");
                return false;
            }
            context.Attach(ctxEntity);
            
            var a = entity.GetType().GetProperties()
                .Where(x => x.GetType().IsPrimitive || x.GetType() == typeof(decimal) || x.GetType() == typeof(string))
                //.Where(x => !x.GetType().IsClass && !x.GetType().IsCollectible)
                .Select(x => x.Name)
                .Where(x => !x.Equals("Id"))
                .ToArray();

            foreach (var prop in entity.GetType().GetProperties()
                .Where(x => !x.GetType().IsClass && !x.GetType().IsCollectible)
                .Select(x => x.Name)
                .Where(x => !x.Equals("Id")))
            {
                context.Entry(entity).Property(prop).IsModified = true;
            }

            return await SaveAsync(info, RepositoryActionType.update);
        }
        public async Task<bool> DeleteAsync(TId entityId, string info)
        {
            var ctxEntity = await context.Set<TEntity>().FindAsync(entityId);

            if (ctxEntity is null)
            {
                Console.WriteLine("data to delete not found");
                return false;
            }

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
                savingError += ex.InnerException.Message;
            }

            if (result >= 0)
            {
                Console.WriteLine($"'{info}' has been {actionName}.");
                return true;
            }
            else
            {
                Console.WriteLine($"'{info}' has not been {actionName}! \n{savingError}");
                return false;
            }
        }
    }
}
