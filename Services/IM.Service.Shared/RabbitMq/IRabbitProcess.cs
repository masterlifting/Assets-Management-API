using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Shared.RabbitMq;

public interface IRabbitProcess
{
    Task ProcessAsync<T>(QueueActions action, T model) where T : class;
    Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class;
}