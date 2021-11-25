using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices
{
    public interface IDataLoad<TEntity, in TDataConfig> where TEntity : class where TDataConfig : class
    {
        Task<TEntity[]> DataSetAsync(string companyId);
        Task<TEntity[]> DataSetAsync();

         Task<TEntity?> GetLastDatabaseDataAsync(string companyId);
         Task<TEntity[]> GetLastDatabaseDataAsync();

         Task<TEntity[]> DataGetAsync(string source, TDataConfig config);
         Task<TEntity[]> DataGetAsync(string source, IEnumerable<TDataConfig> config);
    }
}
