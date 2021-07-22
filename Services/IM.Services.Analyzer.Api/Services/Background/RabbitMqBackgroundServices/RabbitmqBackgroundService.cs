using IM.Services.Analyzer.Api.Services.Background.RabbitMqBackgroundServices.Interfaces;
using IM.Services.Analyzer.Api.Settings;
using IM.Services.Analyzer.Api.Settings.Connection;
using IM.Services.Analyzer.Api.Settings.Mq;
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
        private readonly QueueModel queueAnalyzer;

        private readonly IServiceProvider services;
        private readonly IConnection connection;
        private readonly IModel channel;
        public RabbitmqBackgroundService(IServiceProvider services, IOptions<ServiceSettings> options)
        {
            this.services = services;

            mqConnection = new SettingsConverter<ConnectionModel>(options.Value.ConnectionStrings.Mq).Model;
            queueAnalyzer = new SettingsConverter<QueueModel>(options.Value.MqSettings.QueueAnalyzer).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(
                    queueAnalyzer.Name
                    , false
                    , false
                    , false
                    , null);
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

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                using var scope = services.CreateScope();
                var manager = scope.ServiceProvider.GetRequiredService<IRabbitMqManager>();

                manager.CreateTickerAsync(message);
            };

            channel.BasicConsume(queueAnalyzer.Name, true, consumer);

            return Task.CompletedTask;
        }
        public override Task StopAsync(CancellationToken stoppingToken)
        {
            base.StopAsync(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
