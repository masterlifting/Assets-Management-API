using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Recommendations.Services.RabbitMq.Actions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IM.Service.Recommendations.Services.RabbitMq;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(ILogger<RabbitService> logger, IServiceScopeFactory scopeFactory) : base(logger, new()
    {
        { QueueExchanges.Sync, new RabbitSync(scopeFactory) },
        { QueueExchanges.Transfer, new RabbitTransfer(scopeFactory) },
    })
    { }
}