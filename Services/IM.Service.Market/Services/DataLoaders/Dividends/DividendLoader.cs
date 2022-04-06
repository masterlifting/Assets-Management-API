using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Services.DataLoaders.Dividends;

public class DividendLoader : DataLoader<Dividend>
{
    public DividendLoader(
        ILogger<DataLoader<Dividend>> logger,
        Repository<Dividend> repository)
        : base(logger, repository, new Dictionary<byte, IDataGrabber>())
    {
    }
}