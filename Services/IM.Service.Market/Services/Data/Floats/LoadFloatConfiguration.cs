using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data.Floats.Implementations;

namespace IM.Service.Market.Services.Data.Floats;

public sealed class LoadFloatConfiguration : IDataLoaderConfiguration<Float>
{
    public Func<Float, bool> IsCurrentDataCondition { get; }
    public IEqualityComparer<Float> Comparer { get; }
    public ILastDataHelper<Float> LastDataHelper { get; }
    public DataGrabber<Float> Grabber { get; }
 
    public LoadFloatConfiguration(Repository<Float> repository, InvestingClient investingClient)
    {
        Grabber = new(new()
        {
            {(byte) Enums.Sources.Investing, new InvestingGrabber(investingClient)}
        });
        IsCurrentDataCondition = x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow);
        Comparer = new DataDateComparer<Float>();
        LastDataHelper = new LastDateHelper<Float>(repository, 1600);
    }
}