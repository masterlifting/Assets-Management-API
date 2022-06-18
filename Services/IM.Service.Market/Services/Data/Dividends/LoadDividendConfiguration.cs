using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Tasks;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.Data.Dividends;

public sealed class LoadDividendConfiguration : IDataLoaderConfiguration<Dividend>
{
    public Func<Dividend, bool> IsCurrentDataCondition { get; }
    public IEqualityComparer<Dividend> Comparer { get; }
    public ILastDataHelper<Dividend> LastDataHelper { get; }
    public DataGrabber<Dividend> Grabber { get; }

    public LoadDividendConfiguration(IOptions<ServiceSettings> options, Repository<Dividend> repository)
    {
        var settings = options.Value.LoadData.Tasks.FirstOrDefault(x => x.Name.Equals(nameof(LoadDividendTask), StringComparison.OrdinalIgnoreCase)) ?? new();

        Grabber = new (new ());
        IsCurrentDataCondition = _ => true;
        Comparer = new DataDateComparer<Dividend>();
        LastDataHelper = new LastDateHelper<Dividend>(repository, settings.DaysAgo);
    }
}