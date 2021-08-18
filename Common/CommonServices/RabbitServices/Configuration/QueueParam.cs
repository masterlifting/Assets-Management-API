using System;

namespace CommonServices.RabbitServices.Configuration
{
    public class QueueParam
    {
        public QueueParam(QueueEntities entity)
        {
            Entity = entity.ToString();
        }
        public string Entity { get; }
        public QueueActions[] Actions { get; set; } = Array.Empty<QueueActions>();
    }
}
