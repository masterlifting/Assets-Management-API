using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices.Configuration;

namespace IM.Service.Common.Net.RabbitServices;

public interface IRabbitActionService
{
    Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data);
}