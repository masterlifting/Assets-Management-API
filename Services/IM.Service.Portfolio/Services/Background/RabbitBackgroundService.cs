using IM.Service.Portfolio.Services.RabbitMq;
using IM.Service.Portfolio.Settings;
using IM.Service.Shared.RabbitMq;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Threading;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.Background;

public class RabbitBackgroundService : BackgroundService
{
    private readonly RabbitAction action;
    private readonly RabbitSubscriber subscriber;

    public RabbitBackgroundService(RabbitAction action, ILogger<RabbitBackgroundService> logger, IOptions<ServiceSettings> options)
    {
        this.action = action;
        subscriber = new RabbitSubscriber(
            logger,
            options.Value.ConnectionStrings.Mq,
            new[] { QueueExchanges.Sync, QueueExchanges.Function, QueueExchanges.Transfer },
            QueueNames.Portfolio);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            subscriber.Unsubscribe();
            return Task.CompletedTask;
        }

        subscriber.Subscribe(action.StartAsync);
        return Task.CompletedTask;
    }
    public override Task StopAsync(CancellationToken stoppingToken)
    {
        base.StopAsync(stoppingToken);
        subscriber.Unsubscribe();
        return Task.CompletedTask;
    }
}