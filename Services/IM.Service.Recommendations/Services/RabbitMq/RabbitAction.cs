using IM.Service.Shared.RabbitMq;
using IM.Service.Recommendations.Services.RabbitMq.Sync;
using IM.Service.Recommendations.Services.RabbitMq.Transfer;
using IM.Service.Recommendations.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IM.Service.Recommendations.Services.RabbitMq;

public class RabbitAction : RabbitActionBase
{
    public RabbitAction(IOptions<ServiceSettings> options, ILogger<RabbitAction> logger, IServiceScopeFactory scopeFactory) : base(options.Value.ConnectionStrings.Mq, logger, new()

    {
        { QueueExchanges.Sync, new RabbitSync(scopeFactory) },
        { QueueExchanges.Transfer, new RabbitTransfer(scopeFactory) },
    })
    { }
}