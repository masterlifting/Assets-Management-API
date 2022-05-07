using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Portfolio.Services.RabbitMq.Actions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IM.Service.Portfolio.Services.RabbitMq;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(ILogger<RabbitService> logger, IServiceScopeFactory scopeFactory) : base(logger, new()
    {
        { QueueExchanges.Function, new RabbitFunction(scopeFactory) },
        { QueueExchanges.Sync, new RabbitSync(scopeFactory) }
    })
    {
    }
}