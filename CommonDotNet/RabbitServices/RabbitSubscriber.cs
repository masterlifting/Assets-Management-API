
using CommonServices.Models;
using CommonServices.ParserServices;
using CommonServices.RabbitServices.Configuration;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServices.RabbitServices
{
    public class RabbitSubscriber
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly IModel channel;
        private readonly IConnection connection;
        private readonly List<Queue> queues;
        private readonly string[] queuesWithConfirm;

        public RabbitSubscriber(string connectionString, IEnumerable<QueueExchanges> exchangeNames, IReadOnlyCollection<QueueNames> queueNames)
        {
            var mqConnection = new SettingsConverter<ConnectionModel>(connectionString).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            queues = new List<Queue>(queueNames.Count);

            foreach (var exchange in QueueConfiguration.Exchanges.Join(exchangeNames, x => x.NameEnum, y => y, (x, _) => x))
            {
                channel.ExchangeDeclare(exchange.NameString, exchange.Type);

                foreach (var queue in exchange.Queues.Join(queueNames, x => x.NameEnum, y => y, (x, _) => x))
                {
                    if (queues.Contains(queue, new QueueComparer()))
                        continue;

                    channel.QueueDeclare(queue.NameString, false, false, false, null);
                    queues.Add(queue);

                    foreach (var route in queue.Params)
                        channel.QueueBind(queue.NameString, exchange.NameString, $"{queue.NameString}.{route.EntityNameString}.*");
                }
            }

            queuesWithConfirm = queues.Where(x => x.WithConfirm).Select(x => x.NameString).ToArray();
        }

        public void Subscribe(Func<QueueExchanges, string, string, Task<bool>> getActionResult)
        {
            foreach (var queue in queues)
                SubscribeQueue(getActionResult, queue);
        }
        public void Unsubscribe()
        {
            channel.Dispose();
            connection.Dispose();
        }
        private void SubscribeQueue(Func<QueueExchanges, string, string, Task<bool>> getActionResult, Queue queue)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnConsumerOnReceivedAsync;
            channel.BasicConsume(queue.NameString, queue.IsAutoAck, consumer);

            async void OnConsumerOnReceivedAsync(object? _, BasicDeliverEventArgs ea)
            {
                var data = Encoding.UTF8.GetString(ea.Body.ToArray());
                string queueName = string.Empty;
                var result = false;

                try
                {
                    await semaphore.WaitAsync();

                    queueName = ea.RoutingKey.Split('.')[0];
                    var exchange = Enum.Parse<QueueExchanges>(ea.Exchange, true);
                    result = await getActionResult(exchange, ea.RoutingKey, data);

                    semaphore.Release();
                }
                catch (Exception ex)
                {
                    semaphore.Release();

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                if (queuesWithConfirm.Contains(queueName) && result)
                    channel.BasicAck(ea.DeliveryTag, false);
            }
        }
    }
}
