using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Services.Data.Splits;

public sealed class SplitLoader : DataLoader<Split>
{
    public SplitLoader(ILogger<SplitLoader> logger, Repository<Split> repository)
        : base(
            logger,
            repository,
            new Dictionary<byte, IDataGrabber<Split>>(),
            isCurrentDataCondition: _ => true,
            timeAgo: 1,
            comparer: new DataDateComparer<Split>())
    {
    }
}