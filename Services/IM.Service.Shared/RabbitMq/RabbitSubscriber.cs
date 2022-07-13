using IM.Service.Shared.Helpers;
using IM.Service.Shared.Models.Configuration;
using IM.Service.Shared.Models.RabbitMq;

using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Shared.RabbitMq;

public sealed class RabbitSubscriber
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    private readonly IModel channel;
    private readonly IConnection connection;

    private readonly ILogger logger;
    private readonly Queue queue;

    public RabbitSubscriber(ILogger logger, string connectionString, IEnumerable<QueueExchanges> exchangeNames, QueueNames queueName)
    {
        this.logger = logger;

        var exchanges = QueueConfiguration.Exchanges.Join(exchangeNames, x => x.NameEnum, y => y, (x, _) => x).ToArray();
        var queues = exchanges.SelectMany(x => x.Queues).Distinct(new QueueComparer());

        queue = queues.FirstOrDefault(x => x.NameEnum == queueName) ?? throw new NullReferenceException($"{nameof(RabbitSubscriber)}.Error: '{queueName}' was not register");

        var mqConnection = new SettingsConverter<ConnectionModel>(connectionString).Model;

        var factory = new ConnectionFactory
        {
            HostName = mqConnection.Server,
            UserName = mqConnection.UserId,
            Password = mqConnection.Password
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();

        foreach (var exchange in exchanges)
        {
            channel.ExchangeDeclare(exchange.NameString, exchange.Type);

            channel.QueueDeclare(queue.NameString, false, false, false, null);

            foreach (var route in queue.Entities)
                channel.QueueBind(queue.NameString, exchange.NameString, $"{queue.NameString}.{route.NameString}.*");
        }
    }

    public void Subscribe(Func<QueueExchanges, string, string, Task<bool>> queueFunc)
    {
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += OnReceivedAsync;
        channel.BasicConsume(queue.NameString, !queue.WithConfirm, consumer);

        async void OnReceivedAsync(object? _, BasicDeliverEventArgs ea)
        {
            var data = Encoding.UTF8.GetString(ea.Body.ToArray());
            var result = false;

            await semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                var exchange = Enum.Parse<QueueExchanges>(ea.Exchange, true);
                result = await queueFunc(exchange, ea.RoutingKey, data);
            }
            catch (Exception exception)
            {
                logger.LogError($"{nameof(RabbitSubscriber)}.{nameof(Subscribe)}", exception);
            }

            semaphore.Release();

            if (queue.WithConfirm && result)
                channel.BasicAck(ea.DeliveryTag, false);
        }
    }
    public void Unsubscribe()
    {
        channel.Dispose();
        connection.Dispose();
    }
}