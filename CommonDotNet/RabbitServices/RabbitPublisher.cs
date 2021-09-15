
using CommonServices.Models;
using CommonServices.ParserServices;
using CommonServices.RabbitServices.Configuration;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonServices.RabbitServices
{
    public class RabbitPublisher
    {
        private readonly IModel channel;
        private readonly QueueExchange exchange;

        public RabbitPublisher(string connectionString, QueueExchanges exchangeName)
        {
            var currentExchange = QueueConfiguration.Exchanges.FirstOrDefault(x => x.NameEnum == exchangeName);

            exchange = currentExchange ?? throw new NullReferenceException(nameof(exchangeName));

            var mqConnection = new SettingsConverter<ConnectionModel>(connectionString).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };

            IConnection connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(currentExchange.NameString, currentExchange.Type);

            foreach (var queue in currentExchange.Queues.Distinct(new QueueComparer()))
            {
                channel.QueueDeclare(queue.NameString, false, false, false, null);

                foreach (var route in queue.Params)
                    channel.QueueBind(queue.NameString, currentExchange.NameString, $"{queue.NameString}.{route.EntityNameString}.*");
            }
        }

        public void PublishTask(IEnumerable<QueueNames> queues, QueueEntities entity, QueueActions action, string data)
        {
            foreach (var queue in exchange.Queues.Join(queues, x => x.NameEnum, y => y, (x, _) => x))
                foreach (var routingKey in queue.Params.Where(x => x.EntityNameEnum == entity && x.Actions.Contains(action)))
                    channel.BasicPublish(
                    exchange.NameString
                    , $"{queue.NameString}.{routingKey.EntityNameString}.{action}"
                    , null
                    , Encoding.UTF8.GetBytes(data));
        }
        public void PublishTask(IEnumerable<QueueNames> queues, QueueEntities entity, QueueActions action, string[] data)
        {
            foreach (var queue in exchange.Queues.Join(queues, x => x.NameEnum, y => y, (x, _) => x))
                foreach (var routingKey in queue.Params.Where(x => x.EntityNameEnum == entity && x.Actions.Contains(action)))
                    foreach (var d in data)
                        channel.BasicPublish(
                        exchange.NameString
                        , $"{queue.NameString}.{routingKey.EntityNameString}.{action}"
                        , null
                        , Encoding.UTF8.GetBytes(d));
        }

        public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, string data)
        {
            var currentQueue = exchange.Queues.FirstOrDefault(x => x.NameEnum == queue);

            if (currentQueue is null)
                throw new NullReferenceException($"{nameof(queue)} is null");

            var queueParams = currentQueue.Params.FirstOrDefault(x => x.EntityNameEnum == entity && x.Actions.Contains(action));

            if (queueParams is null)
                throw new NullReferenceException($"{nameof(queueParams)} is null");

            channel.BasicPublish(
            exchange.NameString
            , $"{currentQueue.NameString}.{queueParams.EntityNameString}.{action}"
            , null
            , Encoding.UTF8.GetBytes(data));
        }
        public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, IEnumerable<string> data)
        {
            var currentQueue = exchange.Queues.FirstOrDefault(x => x.NameEnum == queue);

            if (currentQueue is null)
                throw new NullReferenceException($"{nameof(queue)} is null");

            var queueParams = currentQueue.Params.FirstOrDefault(x => x.EntityNameEnum == entity && x.Actions.Contains(action));

            if (queueParams is null)
                throw new NullReferenceException($"{nameof(queueParams)} is null");

            foreach (var item in data)
                channel.BasicPublish(
                    exchange.NameString
                    , $"{currentQueue.NameString}.{queueParams.EntityNameString}.{action}"
                    , null
                    , Encoding.UTF8.GetBytes(item));
        }
    }
}
