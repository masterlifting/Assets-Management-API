using System.Threading.Tasks;

namespace IM.Service.Shared.RabbitMq;

public interface IRabbitAction
{
    Task GetResultAsync(QueueEntities entity, QueueActions action, string data);
}