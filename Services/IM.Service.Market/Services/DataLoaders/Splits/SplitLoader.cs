using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Services.DataLoaders.Splits;

public class SplitLoader : DataLoader<Split>
{
    public SplitLoader(ILogger<DataLoader<Split>> logger, Repository<Split> repository)
        : base(logger, repository, new Dictionary<byte, IDataGrabber<Split>>())
    {
        Comparer = new DataDateComparer<Split>();
    }
}