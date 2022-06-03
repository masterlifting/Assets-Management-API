using IM.Service.Shared.RabbitMq;
using IM.Service.Recommendations.Services.RabbitMq.Sync;
using IM.Service.Recommendations.Services.RabbitMq.Transfer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IM.Service.Recommendations.Services.RabbitMq;

public class RabbitAction : RabbitActionResult
{
    public RabbitAction(ILogger logger, IServiceScopeFactory scopeFactory) : base(logger, new()
    {
        { QueueExchanges.Sync, new RabbitSync(scopeFactory) },
        { QueueExchanges.Transfer, new RabbitTransfer(scopeFactory) },
    })
    { }
}