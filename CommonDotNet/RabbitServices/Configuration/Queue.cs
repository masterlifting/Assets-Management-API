using System;

namespace CommonServices.RabbitServices.Configuration
{
    public class Queue
    {
        public Queue(QueueNames name, bool isAutoAck = false, bool withConfirm = false)
        {
            NameString = name.ToString();
            NameEnum = name;
            IsAutoAck = isAutoAck;
            WithConfirm = withConfirm;
        }

        public string NameString { get; }
        public QueueNames NameEnum { get; }
        public bool IsAutoAck { get; }
        public bool WithConfirm { get; }
        public QueueParam[] Params { get; set; } = Array.Empty<QueueParam>();
    }
}
