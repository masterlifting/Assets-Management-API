using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Portfolio.Services.MqServices.Implementations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IM.Service.Portfolio.Services.MqServices;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(ILogger<RabbitService> logger, IServiceScopeFactory scopeFactory) : base(logger, new()
    {
        { QueueExchanges.Function, new RabbitFunctionService(scopeFactory) },
        { QueueExchanges.Sync, new RabbitSyncService(scopeFactory) }
    })
    {
    }
}