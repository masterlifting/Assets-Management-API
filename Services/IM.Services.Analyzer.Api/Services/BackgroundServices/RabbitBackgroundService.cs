using CommonServices.RabbitServices;

using IM.Services.Analyzer.Api.Services.RabbitServices;
using IM.Services.Analyzer.Api.Settings;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.BackgroundServices
{
    public class RabbitBackgroundService : BackgroundService
    {
        private readonly RabbitSubscriber subscriber;
        private readonly IServiceProvider services;

        public RabbitBackgroundService(IServiceProvider services, IOptions<ServiceSettings> options)
        {
            var targetExchanges = new[] { QueueExchanges.crud, QueueExchanges.calculator };
            var targetQueues = new[] { QueueNames.companiesanalyzer };
            subscriber = new RabbitSubscriber(options.Value.ConnectionStrings.Mq, targetExchanges, targetQueues);
            this.services = services;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                subscriber.Unsubscribe();
                return Task.CompletedTask;
            }

            var actionService = services.CreateScope().ServiceProvider.GetRequiredService<RabbitActionService>();
            subscriber.Subscribe(actionService.GetActionResultAsync);

            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            subscriber.Unsubscribe();
            return Task.CompletedTask;
        }
    }
}
