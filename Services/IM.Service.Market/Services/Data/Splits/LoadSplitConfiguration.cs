using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Services.Data.Splits;

public sealed class LoadSplitConfiguration : IDataLoaderConfiguration<Split>
{
    public Func<Split, bool> IsCurrentDataCondition { get; }
    public IEqualityComparer<Split> Comparer { get; }
    public ILastDataHelper<Split> LastDataHelper { get; }
    public DataGrabber<Split> Grabber { get; }

    public LoadSplitConfiguration(Repository<Split> repository)
    {
        Grabber = new(new());
        IsCurrentDataCondition = _ => true;
        Comparer = new DataDateComparer<Split>();
        LastDataHelper = new LastDateHelper<Split>(repository, 1);
    }
}