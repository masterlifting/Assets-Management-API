using System;

namespace IM.Service.Common.Net.RabbitServices.Configuration
{
    public class QueueParam
    {
        public QueueParam(QueueEntities entity)
        {
            EntityNameString = entity.ToString();
            EntityNameEnum = entity;
        }
        public string EntityNameString { get; }
        public QueueEntities EntityNameEnum { get; }
        public QueueActions[] Actions { get; init; } = Array.Empty<QueueActions>();
    }
}
