using IM.Service.Shared.RabbitMq;
using IM.Service.Market.Services.RabbitMq.Function;

namespace IM.Service.Market.Services.RabbitMq;

public class RabbitAction : RabbitActionResult
{
    public RabbitAction(ILogger logger, IServiceScopeFactory scopeFactory) : base(logger, new()
    {
        { QueueExchanges.Function, new RabbitFunction(scopeFactory) },
    }) { }
}