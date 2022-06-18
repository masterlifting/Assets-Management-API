using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Services.Data;
using IM.Service.Market.Settings.Sections;
using IM.Service.Shared.Background;

namespace IM.Service.Market.Services.Tasks;

public sealed class LoadReportTask : IBackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    public LoadReportTask(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task StartAsync<T>(T settings) where T : BackgroundTaskSettings
    {
        var serviceProvider = scopeFactory.CreateAsyncScope().ServiceProvider;

        var loader = serviceProvider.GetRequiredService<DataLoader<Report>>();
        var companySourcesRepo = serviceProvider.GetRequiredService<Repository<CompanySource>>();

        var _settings = settings as LoadTaskSettings ?? throw new ApplicationException("Report settings not found");
        var sourceIds = _settings.Sources.Select(x => (byte)x).ToArray();

        var companySources = await companySourcesRepo.GetSampleAsync(x => sourceIds.Contains(x.SourceId));

        await loader.LoadAsync(companySources);
    }
}