using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonServices.RepositoryService
{
    public interface IRepository<T> where T : class
    {
        Task<(bool trySuccess, T? checkedEntity)> TryCheckEntityAsync(T entity);
        Task<(bool isSuccess, T[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<T> entities);
        Task<T?> GetAlreadyEntityAsync(T entity);
        IQueryable<T> GetAlreadyEntitiesQuery(IEnumerable<T> entities);
        bool IsUpdate(T contextEntity, T newEntity);
    }
}
