
using CommonServices.RabbitServices.Configuration;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CommonServices.RabbitServices
{
    public static class RabbitHelper
    {
        public static bool TrySerialize<T>(string data, out T? entity) where T : class
        {
            entity = null;

            try
            {
                entity = JsonSerializer.Deserialize<T>(data);
                return true;
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Unserializable! Exception: " + ex.Message);
                return false;
            }
        }
    }
    class ExchangeComparer : IEqualityComparer<QueueExchange>
    {
        public bool Equals(QueueExchange? x, QueueExchange? y) => x!.NameEnum == y!.NameEnum;
        public int GetHashCode([DisallowNull] QueueExchange obj) => obj.GetHashCode();
    }
    class QueueComparer : IEqualityComparer<Queue>
    {
        public bool Equals(Queue? x, Queue? y) => x!.NameEnum == y!.NameEnum;
        public int GetHashCode([DisallowNull] Queue obj) => obj.GetHashCode();
    }
}
