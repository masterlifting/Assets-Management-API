using IM.Service.Shared.Background;

namespace IM.Service.Market.Services.Tasks;

public sealed class LoadDividendTask : IBackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    public LoadDividendTask(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task StartAsync<T>(T settings) where T : BackgroundTaskSettings
    {
        throw new NotImplementedException();
    }
}