using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Recommendations.DataAccess.Entities;
using IM.Service.Recommendations.DataAccess.Repository;
using IM.Service.Recommendations.Services.MqServices.Implementations;

using Microsoft.Extensions.Logging;

namespace IM.Service.Recommendations.Services.MqServices;

public class RabbitActionService : RabbitService
{
    public RabbitActionService(ILogger<RabbitService> logger, Repository<Company> companyRepository) : base(logger, new ()
        {
            { QueueExchanges.Sync, new RabbitSyncService(companyRepository) },
        })
    { }
}