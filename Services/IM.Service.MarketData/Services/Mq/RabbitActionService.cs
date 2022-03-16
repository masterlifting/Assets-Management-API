using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.MarketData.Services.Mq.Actions;

namespace IM.Service.MarketData.Services.Mq;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(IServiceScopeFactory scopeFactory) : base(
        new()
        {
            { QueueExchanges.Function, new RabbitFunctions(scopeFactory) }
        })
    {
    }
}