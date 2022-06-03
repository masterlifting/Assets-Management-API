using IM.Service.Shared.Background;

namespace IM.Service.Market.Services.Tasks;

public sealed class LoadReportTask : IBackgroundService
{
    private readonly ILogger logger;
    private readonly IServiceScopeFactory scopeFactory;
    public LoadReportTask(ILogger logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    public Task StartAsync<T>(T settings) where T : BackgroundTaskSettings
    {
        throw new NotImplementedException();
    }
}