using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Common.Net.RepositoryService;

public interface IRepositoryHandler<T> where T : class
{
    Task GetCreateHandlerAsync(ref T entity);
    Task GetCreateHandlerAsync(ref T[] entities);

    Task GetUpdateHandlerAsync(ref T entity);
    Task GetUpdateHandlerAsync(ref T[] entities);

    Task<IList<T>> GetDeleteHandlerAsync(IReadOnlyCollection<T> entities);

    Task SetPostProcessAsync(T entity);
    Task SetPostProcessAsync(IReadOnlyCollection<T> entities);
}