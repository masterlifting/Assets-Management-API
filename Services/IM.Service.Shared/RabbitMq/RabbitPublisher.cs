using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IM.Service.Shared.Helpers;
using IM.Service.Shared.Models.Configuration;
using static IM.Service.Shared.Helpers.ServiceHelper;
using IM.Service.Shared.Models.RabbitMq;

namespace IM.Service.Shared.RabbitMq;

public class RabbitPublisher : IDisposable
{
    private readonly IModel channel;
    private readonly IConnection connection;

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

        connection = factory.CreateConnection();
        channel = connection.CreateModel();

        channel.ExchangeDeclare(currentExchange.NameString, currentExchange.Type);

        foreach (var queue in currentExchange.Queues.Distinct(new QueueComparer()))
        {
            channel.QueueDeclare(queue.NameString, false, false, false, null);

            foreach (var route in queue.Entities)
                channel.QueueBind(queue.NameString, currentExchange.NameString, $"{queue.NameString}.{route.NameString}.*");
        }
    }

    public void PublishTask(QueueNames queue, QueueEntities entity, QueueActions action, object data)
    {
        var _data = JsonHelper.Serialize(data);

        var currentQueue = exchange.Queues.FirstOrDefault(x => x.NameEnum == queue);

        if (currentQueue is null)
            return;

        var queueEntity = currentQueue.Entities.FirstOrDefault(x => x.NameEnum == entity && x.Actions.Contains(action));

        if (queueEntity is null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Entity '{entity}' for Action '{action}' from Queue '{queue}' by Exchange '{exchange.NameString}' not found!");
            Console.ForegroundColor = ConsoleColor.Gray;
            return;
        }
        channel.BasicPublish(
            exchange.NameString
            , $"{currentQueue.NameString}.{queueEntity.NameString}.{action}"
            , null
            , Encoding.UTF8.GetBytes(_data));
    }
    public void PublishTask<T>(QueueNames queue, QueueEntities entity, QueueActions action, T data) where T : class
    {
        var _data = JsonHelper.Serialize(data);

        var currentQueue = exchange.Queues.FirstOrDefault(x => x.NameEnum == queue);

        if (currentQueue is null)
            return;

        var queueEntity = currentQueue.Entities.FirstOrDefault(x => x.NameEnum == entity && x.Actions.Contains(action));

        if (queueEntity is null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Entity '{entity}' for Action '{action}' from Queue '{queue}' by Exchange '{exchange.NameString}' not found!");
            Console.ForegroundColor = ConsoleColor.Gray;
            return;
        }
        channel.BasicPublish(
            exchange.NameString
            , $"{currentQueue.NameString}.{queueEntity.NameString}.{action}"
            , null
            , Encoding.UTF8.GetBytes(_data));
    }
    public void PublishTask<T>(IEnumerable<QueueNames> queues, QueueEntities entity, QueueActions action, T data) where T : class
    {
        var _data = JsonHelper.Serialize(data);

        foreach (var queue in exchange.Queues.Join(queues, x => x.NameEnum, y => y, (x, _) => x))
            foreach (var queueEntity in queue.Entities.Where(x => x.NameEnum == entity && x.Actions.Contains(action)))
                channel.BasicPublish(
                    exchange.NameString
                    , $"{queue.NameString}.{queueEntity.NameString}.{action}"
                    , null
                    , Encoding.UTF8.GetBytes(_data));
    }

    public void Dispose()
    {
        channel.Dispose();
        connection.Dispose();
    }
}