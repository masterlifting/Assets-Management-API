using IM.Service.Shared.RabbitMq;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Market.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.Background;

public class RabbitBackgroundService : BackgroundService
{
    private readonly RabbitAction action;
    private readonly RabbitSubscriber subscriber;

    public RabbitBackgroundService(ILogger<RabbitBackgroundService> logger, IOptions<ServiceSettings> options, IServiceScopeFactory scopeFactory)
    {
        subscriber = new RabbitSubscriber(
            logger,
            options.Value.ConnectionStrings.Mq,
            new[] { QueueExchanges.Sync, QueueExchanges.Function, QueueExchanges.Transfer },
            QueueNames.Market);
        action = new RabbitAction(logger, scopeFactory);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            subscriber.Unsubscribe();
            return Task.CompletedTask;
        }

        subscriber.Subscribe(action.GetResultAsync);

        return Task.CompletedTask;
    }
    public override Task StopAsync(CancellationToken stoppingToken)
    {
        base.StopAsync(stoppingToken);
        subscriber.Unsubscribe();
        return Task.CompletedTask;
    }
}