using CommonServices.RabbitServices;

using IM.Services.Companies.Reports.Api.Services.RabbitServices;

using Microsoft.Extensions.Hosting;

using System.Threading;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.BackgroundServices
{
    public class RabbitBackgroundService : BackgroundService
    {
        private readonly RabbitService queueService;
        private readonly RabbitActionService rabbitmqService;

        public RabbitBackgroundService(RabbitService queueService, RabbitActionService rabbitmqService)
        {
            this.queueService = queueService;
            this.rabbitmqService = rabbitmqService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                queueService.Stop();
                return Task.CompletedTask;
            }

            queueService.GetSubscruber().Subscribe(rabbitmqService.GetActionResultAsync);

            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            queueService.Stop();
            return Task.CompletedTask;
        }
    }
}
