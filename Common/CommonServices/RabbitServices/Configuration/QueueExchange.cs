using System;

namespace CommonServices.RabbitServices.Configuration
{
    public class QueueExchange
    {
        public QueueExchange(QueueExchanges name, string type)
        {
            Name = name.ToString();
            Type = string.IsNullOrWhiteSpace(type) ? "topic" : type;
        }
        public string Name { get; }
        public string Type { get; }

        public Queue[] Queues { get; set; } = Array.Empty<Queue>();
    }
}
