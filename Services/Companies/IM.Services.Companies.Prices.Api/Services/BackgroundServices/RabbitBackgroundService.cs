using CommonServices.RabbitServices;

using IM.Services.Companies.Prices.Api.Services.RabbitServices;

using Microsoft.Extensions.Hosting;

using System.Threading;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.BackgroundServices
{
    public class RabbitBackgroundService : BackgroundService
    {
        private readonly RabbitService rabbitService;
        private readonly RabbitActionService actionService;

        public RabbitBackgroundService(RabbitService rabbitService, RabbitActionService actionService)
        {
            this.rabbitService = rabbitService;
            this.actionService = actionService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                rabbitService.Stop();
                return Task.CompletedTask;
            }

            rabbitService.GetSubscruber().Subscribe(actionService.GetActionResultAsync);

            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            rabbitService.Stop();
            return Task.CompletedTask;
        }
    }
}
