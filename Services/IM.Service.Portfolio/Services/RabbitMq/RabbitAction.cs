using IM.Service.Shared.RabbitMq;
using IM.Service.Portfolio.Services.RabbitMq.Function;
using IM.Service.Portfolio.Services.RabbitMq.Sync;
using IM.Service.Portfolio.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IM.Service.Portfolio.Services.RabbitMq;

public class RabbitAction : RabbitActionBase
{
    public RabbitAction(IOptions<ServiceSettings> options, ILogger<RabbitAction> logger, IServiceScopeFactory scopeFactory) : base(options.Value.ConnectionStrings.Mq,  logger, new()
    {
        { QueueExchanges.Function, new RabbitFunction(scopeFactory) },
        { QueueExchanges.Sync, new RabbitSync(scopeFactory) }
    }) { }
}