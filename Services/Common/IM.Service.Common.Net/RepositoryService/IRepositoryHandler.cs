using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Common.Net.RepositoryService
{
    public interface IRepositoryHandler<T> where T : class
    {
        Task GetCreateHandlerAsync(ref T entity);
        Task GetCreateHandlerAsync(ref T[] entities, IEqualityComparer<T> comparer);

        Task GetUpdateHandlerAsync(ref T entity);
        Task GetUpdateHandlerAsync(ref T[] entities);

        Task SetPostProcessAsync(T entity);
        Task SetPostProcessAsync(T[] entities);
    }
}
