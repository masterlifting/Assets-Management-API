using IM.Service.MarketData.Domain.DataAccess;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Domain.Entities.ManyToMany;

namespace IM.Service.MarketData.Services.DataLoaders.Splits;

public class SplitLoader : DataLoader<Split>
{
    public SplitLoader(
        ILogger<DataLoader<Split>> logger,
        Repository<Split> repository,
        Repository<CompanySource> companySourceRepo)
        : base(logger, repository, companySourceRepo, new Dictionary<byte, IDataGrabber>())
    {
    }
}