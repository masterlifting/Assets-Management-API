using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.Background;

public class RabbitBackgroundService : BackgroundService
{
    private readonly RabbitActionService service;
    private readonly RabbitSubscriber subscriber;

    public RabbitBackgroundService(ILogger<RabbitSubscriber> logger, IOptions<ServiceSettings> options, RabbitActionService service)
    {
        var targetExchanges = new[] { QueueExchanges.Sync, QueueExchanges.Function, QueueExchanges.Transfer };
        var targetQueues = new[] { QueueNames.Market};
           
        this.service = service;
        subscriber = new RabbitSubscriber(logger, options.Value.ConnectionStrings.Mq, targetExchanges, targetQueues);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            subscriber.Unsubscribe();
            return Task.CompletedTask;
        }

        subscriber.Subscribe(service.GetActionResultAsync);

        return Task.CompletedTask;
    }
    public override Task StopAsync(CancellationToken stoppingToken)
    {
        base.StopAsync(stoppingToken);
        subscriber.Unsubscribe();
        return Task.CompletedTask;
    }
}