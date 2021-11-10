using System;

namespace IM.Service.Common.Net.RabbitServices.Configuration
{
    public class Queue
    {
        public Queue(QueueNames name, bool withConfirm = false)
        {
            NameString = name.ToString();
            NameEnum = name;
            WithConfirm = withConfirm;
        }

        public string NameString { get; }
        public QueueNames NameEnum { get; }
        public bool WithConfirm { get; }
        public QueueParam[] Params { get; init; } = Array.Empty<QueueParam>();
    }
}
