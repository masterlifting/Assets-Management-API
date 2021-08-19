using RabbitMQ.Client;

using System;

namespace CommonServices.RabbitServices.Configuration
{
    public class QueueExchange
    {
        public QueueExchange(QueueExchanges name, string type = ExchangeType.Topic)
        {
            NameString = name.ToString();
            Type = string.IsNullOrWhiteSpace(type) ? ExchangeType.Topic : type;
            NameEnum = name;
        }
        public string NameString { get; }
        public QueueExchanges NameEnum { get; }
        public string Type { get; }

        public Queue[] Queues { get; set; } = Array.Empty<Queue>();
    }
}
