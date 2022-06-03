using IM.Service.Market.Services.Tasks;

namespace IM.Service.Market.Services.Background.Compute;

public sealed class ComputeRatingBackgroundService : BaseComputeBackgroundService
{
    public ComputeRatingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ComputeRatingBackgroundService> logger)
        : base(scopeFactory, logger, new ComputeRatingTask(scopeFactory))
    {
    }
}