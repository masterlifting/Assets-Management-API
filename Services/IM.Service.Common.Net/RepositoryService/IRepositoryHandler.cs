using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Common.Net.RepositoryService;

public interface IRepositoryHandler<T> where T : class
{
    Task<T> RunCreateHandlerAsync(T entity);
    Task<IEnumerable<T>> RunCreateRangeHandlerAsync(IEnumerable<T> entities, IEqualityComparer<T> comparer);

    Task<T> RunUpdateHandlerAsync(object[] id, T entity);
    Task<T> RunUpdateHandlerAsync(T entity);
    Task<IEnumerable<T>> RunUpdateRangeHandlerAsync(IEnumerable<T> entities);

    Task RunPostProcessAsync(RepositoryActions action, T entity);
    Task RunPostProcessAsync(RepositoryActions action, IReadOnlyCollection<T> entities);

    IQueryable<T> GetExist(IEnumerable<T> entities);
}