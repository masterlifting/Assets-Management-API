using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Services.DataLoaders.Splits;

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