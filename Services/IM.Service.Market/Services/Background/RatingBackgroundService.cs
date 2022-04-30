using IM.Service.Market.Services.Calculations;

using static IM.Service.Common.Net.Helpers.LogHelper;

namespace IM.Service.Market.Services.Background;

public class RatingBackgroundService : BackgroundService
{
    private readonly ILogger<RatingBackgroundService> logger;
    private readonly RatingComparator service;

    public RatingBackgroundService(ILogger<RatingBackgroundService> logger, RatingComparator service)
    {
        this.logger = logger;
        this.service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken cToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(cToken))
        {
            try
            {
                await service.StartAsync();
            }
            catch (Exception exception)
            {
                logger.LogError(nameof(ExecuteAsync), exception.Message);
            }
        }
    }
    public override Task StopAsync(CancellationToken stoppingToken)
    {
        base.StopAsync(stoppingToken);
        return Task.CompletedTask;
    }
}