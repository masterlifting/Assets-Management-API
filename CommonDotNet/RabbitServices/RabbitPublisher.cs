
using CommonServices.Models;
using CommonServices.ParserServices;
using CommonServices.RabbitServices.Configuration;

using RabbitMQ.Client;

using System;
using System.Linq;
using System.Text;

namespace CommonServices.RabbitServices
{
    public class RabbitPublisher
    {
        private readonly IModel channel;
        private readonly IConnection connection;
        private readonly QueueExchange exchange;

        public RabbitPublisher(string connectionString, QueueExchanges exchangeName)
        {
            var exchange = QueueConfiguration.Exchanges.FirstOrDefault(x => x.NameEnum == exchangeName);

            if (exchange is null)
                throw new NullReferenceException(nameof(exchangeName));

            this.exchange = exchange;

            var mqConnection = new SettingsConverter<ConnectionModel>(connectionString).Model;

            var factory = new ConnectionFactory()
            {
                HostName = mqConnection.Server,
                UserName = mqConnection.UserId,
                Password = mqConnection.Password
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange.NameString, exchange.Type);

            foreach (var queue in exchange.Queues.Distinct(new QueueComparer()))
            {
                channel.QueueDeclare(queue.NameString, false, false, false, null);

                foreach (var route in queue.Params)
                    channel.QueueBind(queue.NameString, exchange.NameString, $"{queue.NameString}.{route.EntityNameString}.*");
            }
        }

        public void PublishTask(QueueNames[] queues, QueueEntities entity, QueueActions action, string data)
        {
            foreach (var queue in exchange.Queues.Join(queues, x => x.NameEnum, y => y, (x, y) => x))
                foreach (var routingKey in queue.Params.Where(x => x.EntityNameEnum == entity && x.Actions.Contains(action)))
                    channel.BasicPublish(
                    exchange.NameString
                    , $"{queue.NameString}.{routingKey.EntityNameString}.{action}"
                    , null
                    , Encoding.UTF8.GetBytes(data));
        }
        public void PublishTask(QueueNames[] queues, QueueEntities entity, QueueActions action, string[] data)
        {
            foreach (var queue in exchange.Queues.Join(queues, x => x.NameEnum, y => y, (x, y) => x))
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
            var _queue = exchange.Queues.FirstOrDefault(x => x.NameEnum == queue);

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
            var _queue = exchange.Queues.FirstOrDefault(x => x.NameEnum == queue);

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
}
