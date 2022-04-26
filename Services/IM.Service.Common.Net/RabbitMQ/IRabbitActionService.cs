using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitMQ.Configuration;

namespace IM.Service.Common.Net.RabbitMQ;

public interface IRabbitActionService
{
    Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data);
}