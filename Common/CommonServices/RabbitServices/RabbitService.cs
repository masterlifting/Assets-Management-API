
using CommonServices.RabbitServices.Configuration;

using Microsoft.Extensions.DependencyInjection;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public RabbitService(RabbitBuilder builder, IServiceProvider services)
        {
            this.builder = builder;
            this.services = services;
        }

        public RabbitPublisher GetPublisher(QueueExchanges exchange) => new(builder.Channel, exchange);
        public RabbitSubscriber GetSubscruber() => new(builder.Channel, services);
        public static bool TrySerialize<T>(string data, out T entity) where T : class
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
        private readonly QueueExchange exchange;

        public RabbitPublisher(IModel channel, QueueExchanges exchange)
        {
            var ex = QueueConfiguration.Exchanges.FirstOrDefault(x => x.Name.Equals(exchange.ToString().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));

            if (ex is null)
                throw new NullReferenceException(nameof(exchange));

            this.channel = channel;
            this.exchange = ex;
        }

        public void PublishTask(QueueEntities entity, QueueActions action, string data)
        {
            string sentity = entity.ToString();

            foreach (var queue in exchange.Queues)
                foreach (var routingKey in queue.Params.Where(x => x.Entity.Equals(sentity, StringComparison.OrdinalIgnoreCase) && x.Actions.Contains(action)))
                    channel.BasicPublish(
                    exchange.Name
                    , $"{queue.Name}.{sentity}.{action}"
                    , null
                    , Encoding.UTF8.GetBytes(data));
        }
        public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, string data)
        {
            string squeue = queue.ToString();
            string sentity = entity.ToString();

            var q = exchange.Queues.FirstOrDefault(x => x.Name.Equals(squeue, StringComparison.OrdinalIgnoreCase));

            if (q is null)
                throw new NullReferenceException($"{nameof(squeue)} not found");

            var queueParams = q.Params.FirstOrDefault(x => x.Entity.Equals(sentity, StringComparison.OrdinalIgnoreCase) && x.Actions.Contains(action));

            if (queueParams is null)
                throw new NullReferenceException($"{nameof(queueParams)} not found");

            channel.BasicPublish(
            exchange.Name
            , $"{squeue}.{sentity}.{action}"
            , null
            , Encoding.UTF8.GetBytes(data));
        }
        public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, List<string> data)
        {
            string squeue = queue.ToString();
            string sentity = entity.ToString();


            var q = exchange.Queues.FirstOrDefault(x => x.Name.Equals(squeue, StringComparison.OrdinalIgnoreCase));

            if (q is null)
                throw new NullReferenceException($"{nameof(squeue)} not found");

            var queueParams = q.Params.FirstOrDefault(x => x.Entity.Equals(sentity, StringComparison.OrdinalIgnoreCase) && x.Actions.Contains(action));

            if (queueParams is null)
                throw new NullReferenceException($"{nameof(queueParams)} not found");

            foreach (var item in data)
                channel.BasicPublish(
                    exchange.Name
                    , $"{squeue}.{sentity}.{action}"
                    , null
                    , Encoding.UTF8.GetBytes(item));
        }
    }
    public class RabbitSubscriber
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly IModel channel;
        private readonly IServiceProvider services;

        public RabbitSubscriber(IModel channel, IServiceProvider services)
        {
            this.channel = channel;
            this.services = services;
        }

        public void Subscribe(Func<QueueExchanges, string, string, IServiceScope, Task<bool>> getActionResult)
        {
            var queues = QueueConfiguration.Exchanges.SelectMany(x => x.Queues);
            var uniqQueueNames = queues.Select(x => x.Name).Distinct(new QueueNameComparer());

            foreach (var item in uniqQueueNames)
                if (Enum.TryParse(item, out QueueNames queue))
                    SubscribeQueue(getActionResult, queue);
        }
        private void SubscribeQueue(Func<QueueExchanges, string, string, IServiceScope, Task<bool>> getActionResult, QueueNames queue)
        {
            var consumer = new EventingBasicConsumer(channel);
            bool isAutoAck = queue != QueueNames.companiesreportscrud;

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

                QueueNames q = Enum.Parse<QueueNames>(queueName);

                if (q == QueueNames.companiesreportscrud && result)
                    channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(queue.ToString(), isAutoAck, consumer);
        }
    }

    class QueueNameComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y) => x.ToLowerInvariant().Trim() == y.ToLowerInvariant().Trim();
        public int GetHashCode([DisallowNull] string obj) => obj.GetHashCode();
    }
}
