using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data.Floats.Implementations;

namespace IM.Service.Market.Services.Data.Floats;

public sealed class FloatLoader : DataLoader<Float>
{
    public FloatLoader(ILogger<FloatLoader> logger, Repository<Float> repository, InvestingClient investingClient)
        : base(logger, repository, new Dictionary<byte, IDataGrabber<Float>>
        {
            { (byte)Enums.Sources.Investing, new InvestingGrabber(investingClient) }
        },
        isCurrentDataCondition: x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow),
        timeAgo: 1600,
        comparer: new DataDateComparer<Float>())
    {
    }
}