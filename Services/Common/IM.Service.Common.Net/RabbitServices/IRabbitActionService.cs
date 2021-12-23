using System.Threading.Tasks;

namespace IM.Service.Common.Net.RabbitServices;

public interface IRabbitActionService
{
    Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data);
}