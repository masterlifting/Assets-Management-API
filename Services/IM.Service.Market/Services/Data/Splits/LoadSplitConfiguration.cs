using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Tasks;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.Data.Splits;

public sealed class LoadSplitConfiguration : IDataLoaderConfiguration<Split>
{
    public Func<Split, bool> IsCurrentDataCondition { get; }
    public IEqualityComparer<Split> Comparer { get; }
    public ILastDataHelper<Split> LastDataHelper { get; }
    public DataGrabber<Split> Grabber { get; }

    public LoadSplitConfiguration(IOptions<ServiceSettings> options, Repository<Split> repository)
    {
        var settings = options.Value.LoadData.Tasks.FirstOrDefault(x => x.Name.Equals(nameof(LoadSplitTask), StringComparison.OrdinalIgnoreCase)) ?? new();

        Grabber = new(new());
        IsCurrentDataCondition = _ => true;
        Comparer = new DataDateComparer<Split>();
        LastDataHelper = new LastDateHelper<Split>(repository, settings.DaysAgo);
    }
}