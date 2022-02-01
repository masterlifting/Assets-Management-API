﻿using IM.Service.Common.Net.RabbitServices;
using IM.Service.Company.Analyzer.Services.MqServices.Implementations;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Generic;
using IM.Service.Common.Net.RabbitServices.Configuration;

namespace IM.Service.Company.Analyzer.Services.MqServices;

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