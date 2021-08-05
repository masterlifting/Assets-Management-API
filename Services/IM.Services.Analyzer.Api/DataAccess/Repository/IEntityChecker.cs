using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public interface IEntityChecker<T> where T : class
    {
        Task<bool> IsAlreadyAsync(T entity);
        bool WithError(T entity);
    }
}
