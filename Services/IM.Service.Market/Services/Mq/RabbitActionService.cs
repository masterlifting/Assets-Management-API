using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Market.Services.Mq.Actions;

namespace IM.Service.Market.Services.Mq;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(ILogger<RabbitService> logger, IServiceScopeFactory scopeFactory) : base(logger, new()
    {
        { QueueExchanges.Function, new RabbitFunctions(scopeFactory) }
    })
    {
    }
}