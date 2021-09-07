﻿
using System.Threading.Tasks;

namespace CommonServices.RabbitServices
{
    public interface IRabbitActionService
    {
        Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data);
    }
}