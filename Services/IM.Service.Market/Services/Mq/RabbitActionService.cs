using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Services.Mq.Actions;

namespace IM.Service.Market.Services.Mq;

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