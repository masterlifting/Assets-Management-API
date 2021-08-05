using IM.Services.Analyzer.Api.Settings;
using IM.Services.Analyzer.Api.Settings.Connection;
using IM.Services.Companies.Prices.Api.Settings;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.Background.RabbitMqBackgroundServices
{
    public class RabbitmqBackgroundService : BackgroundService
    {
        private readonly ConnectionModel mqConnection;

        private readonly IServiceProvider services;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string queueName;

        public RabbitmqBackgroundService(IServiceProvider services, IOptions<ServiceSettings> options)
        {
            this.services = services;

            mqConnection = new SettingsConverter<ConnectionModel>(options.Value.ConnectionStrings.Mq).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare("crud", ExchangeType.Topic);
            queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, "crud", "analyzer.*.ticker");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                channel.Dispose();
                connection.Dispose();
                return Task.CompletedTask;
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var data = Encoding.UTF8.GetString(ea.Body.ToArray());
                using var scope = services.CreateScope();

                if (await RabbitmqActionService.GetCrudActionResultAsync(data, ea.RoutingKey, scope))
                    channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(queueName, false, consumer);

            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
