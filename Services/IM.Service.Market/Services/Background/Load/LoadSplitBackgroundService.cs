using IM.Service.Market.Services.Tasks;

namespace IM.Service.Market.Services.Background.Load;

public sealed class LoadSplitBackgroundService : BaseLoadBackgroundService
{
    public LoadSplitBackgroundService(IServiceScopeFactory scopeFactory, ILogger<LoadSplitBackgroundService> logger)
        : base(scopeFactory, logger, new LoadSplitTask(scopeFactory))
    {
    }
}