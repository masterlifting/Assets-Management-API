using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Portfolio.Services.MqServices.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace IM.Service.Portfolio.Services.MqServices;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(IServiceScopeFactory scopeFactory) : base(
        new()
        {
            { QueueExchanges.Function, new RabbitFunctionService(scopeFactory) },
            { QueueExchanges.Sync, new RabbitSyncService(scopeFactory) }
        })
    {
    }
}