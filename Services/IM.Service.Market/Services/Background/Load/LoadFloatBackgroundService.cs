using IM.Service.Market.Services.Tasks;

namespace IM.Service.Market.Services.Background.Load;

public sealed class LoadFloatBackgroundService : BaseLoadBackgroundService
{
    public LoadFloatBackgroundService(IServiceScopeFactory scopeFactory, ILogger<LoadFloatBackgroundService> logger)
        : base(scopeFactory, logger, new LoadFloatTask(scopeFactory))
    {
    }
}