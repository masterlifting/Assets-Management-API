using IM.Service.Shared.RabbitMq;
using IM.Service.Market.Services.RabbitMq.Function;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.RabbitMq;

public class RabbitAction : RabbitActionBase
{
    public RabbitAction(IOptions<ServiceSettings> options, ILogger<RabbitAction> logger, IServiceScopeFactory scopeFactory) : base(options.Value.ConnectionStrings.Mq, logger, new()
    {
        { QueueExchanges.Function, new RabbitFunction(scopeFactory) },
    }) { }
}