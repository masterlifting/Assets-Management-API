using IM.Service.Common.Net.RabbitServices;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Analyzer.Services.MqServices.Implementations;

namespace IM.Service.Market.Analyzer.Services.MqServices;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(IServiceScopeFactory scopeFactory) : base(
        new Dictionary<QueueExchanges, IRabbitActionService>
        {
            { QueueExchanges.Sync, new RabbitSyncService(scopeFactory) },
            { QueueExchanges.Transfer, new RabbitTransferService(scopeFactory) },
            { QueueExchanges.Function, new RabbitFunctionService(scopeFactory) }
        })
    { }
}