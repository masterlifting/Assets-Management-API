using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Services.Data;

public interface IDataGrabber<out TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    IAsyncEnumerable<TEntity[]> GetHistoryDataAsync(CompanySource companySource);
    IAsyncEnumerable<TEntity[]> GetHistoryDataAsync(IEnumerable<CompanySource> companySources);

    IAsyncEnumerable<TEntity[]> GetCurrentDataAsync(CompanySource companySource);
    IAsyncEnumerable<TEntity[]> GetCurrentDataAsync(IEnumerable<CompanySource> companySources);
}