using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.DataLoaders.Floats.Implementations;

namespace IM.Service.Market.Services.DataLoaders.Floats;

public class FloatLoader : DataLoader<Float>
{
    public FloatLoader(ILogger<DataLoader<Float>> logger, Repository<Float> repository, InvestingClient investingClient)
        : base(logger, repository, new Dictionary<byte, IDataGrabber<Float>>
        {
            { (byte)Enums.Sources.Investing, new InvestingGrabber(investingClient) }
        })
    {
        IsCurrentDataCondition = x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow);
        TimeAgo = 1;
        Comparer = new DataDateComparer<Float>();
    }
}