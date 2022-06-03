using IM.Service.Market.Services.Tasks;

namespace IM.Service.Market.Services.Background.Load;

public sealed class LoadReportBackgroundService : BaseLoadBackgroundService
{
    public LoadReportBackgroundService(IServiceScopeFactory scopeFactory, ILogger<LoadReportBackgroundService> logger)
        : base(scopeFactory, logger, new LoadReportTask(logger, scopeFactory))
    {
    }
}