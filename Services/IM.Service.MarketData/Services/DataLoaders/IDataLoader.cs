using IM.Service.MarketData.Domain.Entities.ManyToMany;

namespace IM.Service.MarketData.Services.DataLoaders;

public interface IDataLoader
{
    Task DataSetAsync(CompanySource companySource);
    Task DataSetAsync(IEnumerable<CompanySource> companySources);
    Task DataSetAsync();
}