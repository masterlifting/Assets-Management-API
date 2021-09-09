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
        private readonly IServiceScope scope;

        public RabbitBackgroundService(IServiceProvider services, IOptions<ServiceSettings> options)
        {
            var targetExchanges = new[] { QueueExchanges.Crud, QueueExchanges.Calculator };
            var targetQueues = new[] { QueueNames.CompaniesAnalyzer };
            subscriber = new RabbitSubscriber(options.Value.ConnectionStrings.Mq, targetExchanges, targetQueues);
            scope = services.CreateScope();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                subscriber.Unsubscribe();
                scope.Dispose();
                return Task.CompletedTask;
            }

            var rabbitService = scope.ServiceProvider.GetRequiredService<RabbitActionService>();
            subscriber.Subscribe(rabbitService.GetActionResultAsync);

            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            subscriber.Unsubscribe();
            scope.Dispose();
            return Task.CompletedTask;
        }
    }
}
