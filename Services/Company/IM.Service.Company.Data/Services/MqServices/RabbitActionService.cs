﻿using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Company.Data.Services.MqServices.Implementations;

using Microsoft.Extensions.DependencyInjection;

namespace IM.Service.Company.Data.Services.MqServices;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(IServiceScopeFactory scopeFactory) : base(
        new()
        {
            { QueueExchanges.Sync, new RabbitSyncService(scopeFactory) },
            { QueueExchanges.Transfer, new RabbitTransferService(scopeFactory) },
            { QueueExchanges.Function, new RabbitFunctionService(scopeFactory) }
        })
    {
    }
}