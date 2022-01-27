﻿using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Broker.Data.Services.MqServices.Implementations;

using Microsoft.Extensions.DependencyInjection;

namespace IM.Service.Broker.Data.Services.MqServices;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(IServiceScopeFactory scopeFactory) : base(
        new()
        {
            { QueueExchanges.Function, new RabbitFunctionService(scopeFactory) },
            { QueueExchanges.Sync, new RabbitSyncService(scopeFactory) }
        })
    {
    }
}