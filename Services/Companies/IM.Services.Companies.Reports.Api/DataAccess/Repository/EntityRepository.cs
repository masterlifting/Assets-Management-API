using System;
using System.Threading.Tasks;

using static IM.Services.Companies.Reports.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Reports.Api.DataAccess.Repository
{
    public class EntityRepository<T> where T : class
    {
        private readonly ReportsContext context;
        private readonly IEntityChecker<T> checker;

        public EntityRepository(ReportsContext context, IEntityChecker<T> checker)
        {
            this.context = context;
            this.checker = checker;
        }

        public async Task<bool> CreateAsync(T entity, string info)
        {
            if (checker.WithError(entity))
                return false;

            if (await checker.IsAlreadyAsync(entity))
                return true;

            await context.Set<T>().AddAsync(entity);

            return await SaveAsync(info, RepositoryActionType.create);
        }
        public async Task<bool> UpdateAsync(T entity, string info)
        {
            if (checker.WithError(entity))
                return false;

            context.Set<T>().Update(entity);

            return await SaveAsync(info, RepositoryActionType.update);
        }
        public async Task<bool> DeleteAsync<TId>(TId id, string info)
        {
            var ctxEntity = await context.Set<T>().FindAsync(id);

            if (ctxEntity is null)
            {
                Console.WriteLine($"'{info}' to delete not found");
                return true;
            }

            context.Set<T>().Remove(ctxEntity);

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
