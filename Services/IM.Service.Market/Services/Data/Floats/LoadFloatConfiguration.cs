using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data.Floats.Implementations;
using IM.Service.Market.Services.Tasks;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.Data.Floats;

public sealed class LoadFloatConfiguration : IDataLoaderConfiguration<Float>
{
    public Func<Float, bool> IsCurrentDataCondition { get; }
    public IEqualityComparer<Float> Comparer { get; }
    public ILastDataHelper<Float> LastDataHelper { get; }
    public DataGrabber<Float> Grabber { get; }
 
    public LoadFloatConfiguration(IOptions<ServiceSettings> options, Repository<Float> repository, InvestingClient investingClient)
    {
        var settings = options.Value.LoadData.Tasks.FirstOrDefault(x => x.Name.Equals(nameof(LoadFloatTask), StringComparison.OrdinalIgnoreCase)) ?? new();

        Grabber = new(new()
        {
            {(byte) Enums.Sources.Investing, new InvestingGrabber(investingClient)}
        });
        IsCurrentDataCondition = x => x.Date == DateOnly.FromDateTime(DateTime.UtcNow);
        Comparer = new DataDateComparer<Float>();
        LastDataHelper = new LastDateHelper<Float>(repository, settings.DaysAgo);
    }
}