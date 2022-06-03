using IM.Service.Market.Services.Tasks;

namespace IM.Service.Market.Services.Background.Load;

public sealed class LoadDividendBackgroundService : BaseLoadBackgroundService
{
    public LoadDividendBackgroundService(IServiceScopeFactory scopeFactory, ILogger<LoadDividendBackgroundService> logger)
        : base(scopeFactory, logger, new LoadDividendTask(logger, scopeFactory))
    {
    }
}