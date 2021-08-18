using System;

namespace CommonServices.RabbitServices.Configuration
{
    public class Queue
    {
        public Queue(QueueNames name)
        {
            Name = name.ToString();
        }

        public string Name { get; }
        public QueueParam[] Params { get; set; } = Array.Empty<QueueParam>();
    }
}
