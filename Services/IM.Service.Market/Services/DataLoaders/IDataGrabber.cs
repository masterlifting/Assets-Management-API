using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Services.DataLoaders;

public interface IDataGrabber
{
    Task GetHistoryDataAsync(CompanySource companySource);
    Task GetHistoryDataAsync(IEnumerable<CompanySource> companySources);
           
    Task GetCurrentDataAsync(CompanySource companySource);
    Task GetCurrentDataAsync(IEnumerable<CompanySource> companySources);
}