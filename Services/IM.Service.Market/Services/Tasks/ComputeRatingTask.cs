using IM.Service.Shared.Background;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Entity;

namespace IM.Service.Market.Services.Tasks;

public sealed class ComputeRatingTask : IBackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    public ComputeRatingTask(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task StartAsync<T>(T _) where T : BackgroundTaskSettings
    {
        var serviceProvider = scopeFactory.CreateScope().ServiceProvider;
        var ratingService = serviceProvider.GetRequiredService<RatingService>();

        var tasks = await Task.WhenAll(
            ratingService.CompareAsync<Price>(serviceProvider),
            ratingService.CompareAsync<Report>(serviceProvider),
            ratingService.CompareAsync<Coefficient>(serviceProvider),
            ratingService.CompareAsync<Dividend>(serviceProvider));

        if (tasks.Contains(true))
            await ratingService.ComputeAsync();
    }
}