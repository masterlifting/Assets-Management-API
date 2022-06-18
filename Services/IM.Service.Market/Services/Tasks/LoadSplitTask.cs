using IM.Service.Shared.Background;

namespace IM.Service.Market.Services.Tasks;

public sealed class LoadSplitTask : IBackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    public LoadSplitTask(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task StartAsync<T>(T settings) where T : BackgroundTaskSettings
    {
        throw new NotImplementedException();
    }
}