using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Services.Data.Dividends;

public sealed class LoadDividendConfiguration : IDataLoaderConfiguration<Dividend>
{
    public Func<Dividend, bool> IsCurrentDataCondition { get; }
    public IEqualityComparer<Dividend> Comparer { get; }
    public ILastDataHelper<Dividend> LastDataHelper { get; }
    public DataGrabber<Dividend> Grabber { get; }

    public LoadDividendConfiguration(Repository<Dividend> repository)
    {
        Grabber = new (new ());
        IsCurrentDataCondition = _ => true;
        Comparer = new DataDateComparer<Dividend>();
        LastDataHelper = new LastDateHelper<Dividend>(repository, 1);
    }
}