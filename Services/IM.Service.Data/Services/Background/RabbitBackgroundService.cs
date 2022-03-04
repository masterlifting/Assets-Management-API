﻿using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Data.Services.Mq;
using IM.Service.Data.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Data.Services.Background;

public class RabbitBackgroundService : BackgroundService
{
    private readonly RabbitActionService service;
    private readonly RabbitSubscriber subscriber;

    public RabbitBackgroundService(ILogger<RabbitSubscriber> logger, IOptions<ServiceSettings> options, RabbitActionService service)
    {
        var targetExchanges = new[] { QueueExchanges.Sync, QueueExchanges.Function, QueueExchanges.Transfer };
        var targetQueues = new[] { QueueNames.MarketData};
           
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