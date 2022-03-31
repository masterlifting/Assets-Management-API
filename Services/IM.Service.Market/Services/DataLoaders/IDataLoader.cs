using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Services.DataLoaders;

public interface IDataLoader
{
    Task DataSetAsync(CompanySource companySource);
    Task DataSetAsync(IEnumerable<CompanySource> companySources);
    Task DataSetAsync();
}