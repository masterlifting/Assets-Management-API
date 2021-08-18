using IM.Services.Companies.Prices.Api.Settings;
using IM.Services.Companies.Prices.Api.Settings.Connection;
using IM.Services.Companies.Prices.Api.Settings.Mq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Background.RabbitMqBackgroundServices
{
    public class RabbitmqBackgroundService : BackgroundService
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly ConnectionModel mqConnection;
        private readonly QueueModel queue;

        private readonly IServiceProvider services;
        private readonly IConnection connection;
        private readonly IModel channel;

        public RabbitmqBackgroundService(IServiceProvider services, IOptions<ServiceSettings> options)
        {
            this.services = services;

            mqConnection = new SettingsConverter<ConnectionModel>(options.Value.ConnectionStrings.Mq).Model;
            queue = new SettingsConverter<QueueModel>(options.Value.MqSettings.QueueCompaniesPrices).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare("crud", ExchangeType.Topic);
            channel.QueueDeclare(queue.Name, false, false, false, null);
            channel.QueueBind(queue.Name, "crud", $"{queue.Name}.*.ticker");
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
                bool result = false;

                try
                {
                    await semaphore.WaitAsync();
                    result = await RabbitmqActionService.GetCrudActionResultAsync(data, ea.RoutingKey, scope);
                    semaphore.Release();
                }
                catch (Exception ex)
                {
                    semaphore.Release();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                if (result)
                    channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(queue.Name, false, consumer);

            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
