using System;
using System.Collections.Generic;

using IM.Service.Shared.Models.RabbitMq;

using static IM.Service.Shared.Enums;

namespace IM.Service.Shared.RabbitMq;

public static class RabbitHelper
{
    public static QueueActions GetQueueAction(RepositoryActions action) => action switch
    {
        RepositoryActions.Create => QueueActions.Create,
        RepositoryActions.Update => QueueActions.Update,
        RepositoryActions.Delete => QueueActions.Delete,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}

internal sealed class QueueComparer : IEqualityComparer<Queue>
{
    public bool Equals(Queue? x, Queue? y) => x!.NameEnum == y!.NameEnum;
    public int GetHashCode(Queue obj) => obj.NameString.GetHashCode();
}