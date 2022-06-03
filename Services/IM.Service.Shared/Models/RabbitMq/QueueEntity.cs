using IM.Service.Shared.RabbitMq;

using System;

namespace IM.Service.Shared.Models.RabbitMq;

public class QueueEntity
{
    public QueueEntity(QueueEntities entity)
    {
        NameString = entity.ToString();
        NameEnum = entity;
    }
    public string NameString { get; }
    public QueueEntities NameEnum { get; }
    public QueueActions[] Actions { get; init; } = Array.Empty<QueueActions>();
}