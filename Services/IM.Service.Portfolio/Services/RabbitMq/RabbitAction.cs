using IM.Service.Shared.RabbitMq;
using IM.Service.Portfolio.Services.RabbitMq.Function;
using IM.Service.Portfolio.Services.RabbitMq.Sync;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IM.Service.Portfolio.Services.RabbitMq;

public class RabbitAction : RabbitActionResult
{
    public RabbitAction(ILogger logger, IServiceScopeFactory scopeFactory) : base(logger, new()
    {
        { QueueExchanges.Function, new RabbitFunction(scopeFactory) },
        { QueueExchanges.Sync, new RabbitSync(scopeFactory) }
    }) { }
}