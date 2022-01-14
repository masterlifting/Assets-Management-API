using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IM.Service.Common.Net.Models.Configuration;
using IM.Service.Common.Net.ParserServices;
using IM.Service.Common.Net.RabbitServices.Configuration;

namespace IM.Service.Common.Net.RabbitServices;

public class RabbitPublisher
{
    private readonly IModel channel;
    private readonly QueueExchange exchange;

    public RabbitPublisher(string connectionString, QueueExchanges exchangeName)
    {
        var currentExchange = QueueConfiguration.Exchanges.FirstOrDefault(x => x.NameEnum == exchangeName);

        exchange = currentExchange ?? throw new NullReferenceException(exchangeName.ToString());

        var mqConnection = new SettingsConverter<ConnectionModel>(connectionString).Model;

        var factory = new ConnectionFactory
        {
            HostName = mqConnection.Server,
            UserName = mqConnection.UserId,
            Password = mqConnection.Password
        };

        var connection = factory.CreateConnection();
        channel = connection.CreateModel();

        channel.ExchangeDeclare(currentExchange.NameString, currentExchange.Type);

        foreach (var queue in currentExchange.Queues.Distinct(new QueueComparer()))
        {
            channel.QueueDeclare(queue.NameString, false, false, false, null);

            foreach (var route in queue.Entities)
                channel.QueueBind(queue.NameString, currentExchange.NameString, $"{queue.NameString}.{route.NameString}.*");
        }
    }

    public void PublishTask(IEnumerable<QueueNames> queues, QueueEntities entity, QueueActions action, string data)
    {
        foreach (var queue in exchange.Queues.Join(queues, x => x.NameEnum, y => y, (x, _) => x))
        foreach (var routingKey in queue.Entities.Where(x => x.NameEnum == entity && x.Actions.Contains(action)))
            channel.BasicPublish(
                exchange.NameString
                , $"{queue.NameString}.{routingKey.NameString}.{action}"
                , null
                , Encoding.UTF8.GetBytes(data));
    }
    public void PublishTask(IEnumerable<QueueNames> queues, QueueEntities entity, QueueActions action, string[] data)
    {
        foreach (var queue in exchange.Queues.Join(queues, x => x.NameEnum, y => y, (x, _) => x))
        foreach (var routingKey in queue.Entities.Where(x => x.NameEnum == entity && x.Actions.Contains(action)))
        foreach (var d in data)
            channel.BasicPublish(
                exchange.NameString
                , $"{queue.NameString}.{routingKey.NameString}.{action}"
                , null
                , Encoding.UTF8.GetBytes(d));
    }

    public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, string data)
    {
        var currentQueue = exchange.Queues.FirstOrDefault(x => x.NameEnum == queue);

        if (currentQueue is null)
            throw new NullReferenceException(queue.ToString());

        var queueParams = currentQueue.Entities.FirstOrDefault(x => x.NameEnum == entity && x.Actions.Contains(action));

        if (queueParams is null)
            throw new NullReferenceException(nameof(QueueEntity));

        channel.BasicPublish(
            exchange.NameString
            , $"{currentQueue.NameString}.{queueParams.NameString}.{action}"
            , null
            , Encoding.UTF8.GetBytes(data));
    }
    public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, IEnumerable<string> data)
    {
        var currentQueue = exchange.Queues.FirstOrDefault(x => x.NameEnum == queue);

        if (currentQueue is null)
            throw new NullReferenceException(queue.ToString());

        var queueParams = currentQueue.Entities.FirstOrDefault(x => x.NameEnum == entity && x.Actions.Contains(action));

        if (queueParams is null)
            throw new NullReferenceException(nameof(QueueEntity));

        foreach (var item in data)
            channel.BasicPublish(
                exchange.NameString
                , $"{currentQueue.NameString}.{queueParams.NameString}.{action}"
                , null
                , Encoding.UTF8.GetBytes(item));
    }
}