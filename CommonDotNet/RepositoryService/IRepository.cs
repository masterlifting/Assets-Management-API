using System.Collections.Generic;
using System.Linq;

namespace CommonServices.RepositoryService
{
    public interface IRepository<T> where T : class
    {
        bool TryCheckEntity(T entity, out T? result);
        bool TryCheckEntities(IEnumerable<T> entities, out T[] result);
        T? GetIntersectedContextEntity(T entity);
        IQueryable<T>? GetIntersectedContextEntities(IEnumerable<T> entities);
        bool UpdateEntity(T oldResult, T newResult);
    }
}
