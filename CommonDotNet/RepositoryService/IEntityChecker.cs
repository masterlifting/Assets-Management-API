using System.Threading.Tasks;

namespace CommonServices.RepositoryService
{
    public interface IEntityChecker<T> where T : class
    {
        Task<bool> IsAlreadyAsync(T entity);
        bool WithError(T entity);
    }
}
