using System;
using IM.Service.Common.Net.RabbitServices.Configuration;

using System.Collections.Generic;
using System.Text.Json;
using static IM.Service.Common.Net.CommonEnums;

namespace IM.Service.Common.Net.RabbitServices;

public static class RabbitHelper
{
    public static bool TrySerialize<T>(string data, out T? entity) where T : class
    {
        entity = null;

        try
        {
            entity = JsonSerializer.Deserialize<T>(data, CommonHelper.JsonHelper.Options);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static QueueActions GetQueueAction(RepositoryActions action) => action switch
    {
        RepositoryActions.Create => QueueActions.Create,
        RepositoryActions.CreateUpdate => QueueActions.CreateUpdate,
        RepositoryActions.CreateUpdateDelete => QueueActions.CreateUpdateDelete,
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