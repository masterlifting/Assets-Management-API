using IM.Service.Market.Services.Tasks;

namespace IM.Service.Market.Services.Background.Load;

public sealed class LoadPriceBackgroundService : BaseLoadBackgroundService
{
    public LoadPriceBackgroundService(IServiceScopeFactory scopeFactory, ILogger<LoadPriceBackgroundService> logger)
        : base(scopeFactory, logger, new LoadPriceTask(logger, scopeFactory))
    {
    }
}