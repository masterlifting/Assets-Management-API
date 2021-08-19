
using CommonServices.RabbitServices.Configuration;

using Microsoft.Extensions.DependencyInjection;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CommonServices.RabbitServices
{
    public class RabbitService
    {
        private readonly RabbitBuilder builder;
        private readonly IServiceProvider services;

        public QueueExchange[] Exchanges { get; }
        public Queue[] Queues { get; }

        public RabbitService(RabbitBuilder builder, IServiceProvider services)
        {
            this.builder = builder;
            this.services = services;
            Exchanges = builder.Exchanges;
            Queues = builder.Queues;
        }

        public RabbitPublisher GetPublisher(QueueExchanges exchange) => new(builder, exchange);
        public RabbitSubscriber GetSubscruber() => new(builder, services);
        public static bool TrySerialize<T>(string data, out T? entity) where T : class
        {
            entity = null;

            try
            {
                entity = JsonSerializer.Deserialize<T>(data);
                return true;
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Unserializable! Exception: " + ex.Message);
                return false;
            }
        }
        public void Stop()
        {
            builder.Connection.Dispose();
            builder.Channel.Dispose();
        }
    }

    public class RabbitPublisher
    {
        private readonly IModel channel;
        private readonly Queue[] queues;
        private readonly QueueExchange exchange;

        public RabbitPublisher(RabbitBuilder builder, QueueExchanges exchange)
        {
            var ex = builder.Exchanges.FirstOrDefault(x => x.NameEnum == exchange);

            if (ex is null)
                throw new NullReferenceException(nameof(exchange));

            channel = builder.Channel;
            queues = builder.Queues;
            this.exchange = ex;
        }

        public void PublishTask(QueueEntities entity, QueueActions action, string data)
        {
            foreach (var queue in queues)
                foreach (var routingKey in queue.Params.Where(x => x.EntityNameEnum == entity && x.Actions.Contains(action)))
                    channel.BasicPublish(
                    exchange.NameString
                    , $"{queue.NameString}.{routingKey.EntityNameString}.{action}"
                    , null
                    , Encoding.UTF8.GetBytes(data));
        }
        public void PublishTask(QueueEntities entity, QueueActions action, string[] data)
        {
            foreach (var queue in queues)
                foreach (var routingKey in queue.Params.Where(x => x.EntityNameEnum == entity && x.Actions.Contains(action)))
                    foreach (var _data in data)
                        channel.BasicPublish(
                        exchange.NameString
                        , $"{queue.NameString}.{routingKey.EntityNameString}.{action}"
                        , null
                        , Encoding.UTF8.GetBytes(_data));
        }

        public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, string data)
        {
            var _queue = queues.FirstOrDefault(x => x.NameEnum == queue);

            if (_queue is null)
                throw new NullReferenceException($"{nameof(queue)} not found");

            var queueParams = _queue.Params.FirstOrDefault(x => x.EntityNameEnum == entity && x.Actions.Contains(action));

            if (queueParams is null)
                throw new NullReferenceException($"{nameof(queueParams)} not found");

            channel.BasicPublish(
            exchange.NameString
            , $"{_queue.NameString}.{queueParams.EntityNameString}.{action}"
            , null
            , Encoding.UTF8.GetBytes(data));
        }
        public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, string[] data)
        {
            var _queue = queues.FirstOrDefault(x => x.NameEnum == queue);

            if (_queue is null)
                throw new NullReferenceException($"{nameof(queue)} not found");

            var queueParams = _queue.Params.FirstOrDefault(x => x.EntityNameEnum == entity && x.Actions.Contains(action));

            if (queueParams is null)
                throw new NullReferenceException($"{nameof(queueParams)} not found");

            foreach (var item in data)
                channel.BasicPublish(
                    exchange.NameString
                    , $"{_queue.NameString}.{queueParams.EntityNameString}.{action}"
                    , null
                    , Encoding.UTF8.GetBytes(item));
        }
    }
    public class RabbitSubscriber
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly IModel channel;
        private readonly IServiceProvider services;
        private readonly Queue[] queues;
        private readonly string[] queuesWithConfirm;

        public RabbitSubscriber(RabbitBuilder builder, IServiceProvider services)
        {
            channel = builder.Channel;
            this.services = services;
            queues = builder.Queues;
            queuesWithConfirm = builder.Queues.Where(x => x.WithConfirm).Select(x => x.NameString).ToArray();
        }

        public void Subscribe(Func<QueueExchanges, string, string, IServiceScope, Task<bool>> getActionResult)
        {
            foreach (var queue in queues)
                SubscribeQueue(getActionResult, queue);
        }
        private void SubscribeQueue(Func<QueueExchanges, string, string, IServiceScope, Task<bool>> getActionResult, Queue queue)
        {
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                var data = Encoding.UTF8.GetString(ea.Body.ToArray());
                using var scope = services.CreateScope();
                string queueName = string.Empty;
                bool result = false;

                try
                {
                    await semaphore.WaitAsync();

                    queueName = ea.RoutingKey.Split('.')[0];
                    var exchange = Enum.Parse<QueueExchanges>(ea.Exchange.ToLowerInvariant());
                    result = await getActionResult(exchange, ea.RoutingKey, data, scope);

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
            };

            channel.BasicConsume(queue.NameString, queue.IsAutoAck, consumer);
        }
    }
}
