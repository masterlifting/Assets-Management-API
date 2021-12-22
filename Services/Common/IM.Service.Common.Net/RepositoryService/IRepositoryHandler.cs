using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Common.Net.RepositoryService;

public interface IRepositoryHandler<T> where T : class
{
    Task<T> GetCreateHandlerAsync(T entity);
    Task<T> GetUpdateHandlerAsync(object[] id, T entity);
    Task<T> GetDeleteHandlerAsync(params object[] id);
    Task SetPostProcessAsync(T entity);

    Task<IEnumerable<T>> GetCreateRangeHandlerAsync(IEnumerable<T> entities, IEqualityComparer<T> comparer);
    Task<IEnumerable<T>> GetUpdateRangeHandlerAsync(IEnumerable<T> entities);
    Task<IEnumerable<T>> GetDeleteRangeHandlerAsync(IEnumerable<T> entities);
    Task SetPostProcessAsync(IReadOnlyCollection<T> entities);

    IQueryable<T> GetExist(IEnumerable<T> entities);
}