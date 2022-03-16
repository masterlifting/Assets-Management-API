using IM.Service.MarketData.Domain.Entities.ManyToMany;

namespace IM.Service.MarketData.Services.DataLoaders;

public interface IDataGrabber
{
    Task GetHistoryDataAsync(CompanySource companySource);
    Task GetHistoryDataAsync(IEnumerable<CompanySource> companySources);
           
    Task GetCurrentDataAsync(CompanySource companySource);
    Task GetCurrentDataAsync(IEnumerable<CompanySource> companySources);
}