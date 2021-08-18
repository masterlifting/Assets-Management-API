using IM.Gateways.Web.Companies.Api.Settings.Mq;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement
{
    public class RabbitAction
    {
        private readonly IModel channel;
        private readonly Exchange exchange;

        public RabbitAction(IModel channel, Exchange exchange)
        {
            this.channel = channel;
            this.exchange = exchange;
        }

        public void SetQueueTask(string entityName, string actionName, string data)
        {
            foreach (var queue in exchange.Queues)
                foreach (var routingKey in queue.Params.Where(x => x.Entity.Equals(entityName, StringComparison.OrdinalIgnoreCase) && x.Actions.Contains(actionName)))
                    channel.BasicPublish(
                    exchange.Name
                    , $"{queue.Name}.{entityName}.{actionName}"
                    , null
                    , Encoding.UTF8.GetBytes(data));
        }
        public void SetQueueTask(string queueName, string entityName, string actionName, string data)
        {
            var queue = exchange.Queues.FirstOrDefault(x => x.Name.Equals(queueName, StringComparison.OrdinalIgnoreCase));

            if (queue is null)
                throw new NullReferenceException($"{nameof(queue)} not found");

            var queueParams = queue.Params.FirstOrDefault(x => x.Entity.Equals(entityName, StringComparison.OrdinalIgnoreCase) && x.Actions.Contains(actionName));

            if (queueParams is null)
                throw new NullReferenceException($"{nameof(queueParams)} not found");

            channel.BasicPublish(
            exchange.Name
            , $"{queueName}.{entityName}.{actionName}"
            , null
            , Encoding.UTF8.GetBytes(data));
        }
        public void SetQueueTask(string queueName, string entityName, string actionName, List<string> data)
        {
            var queue = exchange.Queues.FirstOrDefault(x => x.Name.Equals(queueName, StringComparison.OrdinalIgnoreCase));

            if (queue is null)
                throw new NullReferenceException($"{nameof(queue)} not found");

            var queueParams = queue.Params.FirstOrDefault(x => x.Entity.Equals(entityName, StringComparison.OrdinalIgnoreCase) && x.Actions.Contains(actionName));

            if (queueParams is null)
                throw new NullReferenceException($"{nameof(queueParams)} not found");

            foreach (var item in data)
                channel.BasicPublish(
                    exchange.Name
                    , $"{queueName}.{entityName}.{actionName}"
                    , null
                    , Encoding.UTF8.GetBytes(item));
        }
    }
}
