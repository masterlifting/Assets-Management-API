using IM.Service.Shared.Background;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Services.Data;
using IM.Service.Market.Settings.Sections;

namespace IM.Service.Market.Services.Tasks;

public sealed class LoadPriceTask : IBackgroundService
{
    private readonly ILogger logger;
    private readonly IServiceScopeFactory scopeFactory;
    public LoadPriceTask(ILogger logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    public async Task StartAsync<T>(T settings) where T : BackgroundTaskSettings
    {
        var _settings = settings as LoadTaskSettings ?? throw new ApplicationException("Settings not found");
        var sourceIds = _settings.Sources.Select(x => (byte) x).ToArray();
        
        var serviceProvider =  scopeFactory.CreateAsyncScope().ServiceProvider;
        var loader = serviceProvider.GetRequiredService<DataLoader<Price>>();
        var companySourcesRepo = serviceProvider.GetRequiredService<Repository<CompanySource>>();
        
        var companySources = await companySourcesRepo.GetSampleAsync(x => sourceIds.Contains(x.SourceId));
        
        await loader.LoadAsync(companySources);
    }
}