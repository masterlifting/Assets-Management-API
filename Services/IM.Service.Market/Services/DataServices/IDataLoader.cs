using System.Threading.Tasks;

namespace IM.Service.Market.Services.DataServices;

public interface IDataLoader<TEntity> where TEntity : class
{
    Task<TEntity?> GetLastDataAsync(string companyId);
    Task<TEntity[]> GetLastDataAsync();

    Task DataSetAsync(string companyId);
    Task DataSetAsync();
}