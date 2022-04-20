using IM.Service.Common.Net.RabbitServices.Configuration;

using System;
using System.Collections.Generic;

namespace IM.Service.Common.Net.RabbitServices;

public static class RabbitHelper
{
    public static QueueActions GetQueueAction(Enums.RepositoryActions action) => action switch
    {
        Enums.RepositoryActions.Create => QueueActions.Create,
        Enums.RepositoryActions.Update => QueueActions.Update,
        Enums.RepositoryActions.Delete => QueueActions.Delete,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}

internal sealed class QueueComparer : IEqualityComparer<Queue>
{
    public bool Equals(Queue? x, Queue? y) => x!.NameEnum == y!.NameEnum;
    public int GetHashCode(Queue obj) => obj.NameString.GetHashCode();
}