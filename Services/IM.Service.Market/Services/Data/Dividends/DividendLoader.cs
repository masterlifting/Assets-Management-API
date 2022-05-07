using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Services.Data.Dividends;

public sealed class DividendLoader : DataLoader<Dividend>
{
    public DividendLoader(ILogger<DividendLoader> logger, Repository<Dividend> repository)
        : base(
            logger,
            repository,
            new Dictionary<byte, IDataGrabber<Dividend>>(),
            isCurrentDataCondition: _ => true,
            timeAgo: 1,
            comparer: new DataDateComparer<Dividend>())
    {
    }
}