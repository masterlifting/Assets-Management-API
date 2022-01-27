using IM.Service.Broker.Data.Services.DataServices.Reports;
using IM.Service.Common.Net;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IM.Service.Broker.Data.Models.Dto.Mq;

namespace IM.Service.Broker.Data.Services.MqServices.Implementations;

public class RabbitFunctionService : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctionService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        if (action != QueueActions.Call)
            return true;

        try
        {
            switch (entity)
            {
                case QueueEntities.Report:
                    {
                        if (RabbitHelper.TrySerialize(data, out ReportFileDto? dto))
                        {
                            var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ReportLoader>();
                            await reportLoader.DataSetAsync(dto!);
                            break;
                        }

                        throw new SerializationException(nameof(ReportFileDto));
                    }
            }

            return true;
        }
        catch (Exception exception)
        {
            var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<RabbitFunctionService>>();
            logger.LogError(LogEvents.Call, "Entity: {entity} Queue action: {action} failed! \nError: {error}", Enum.GetName(entity), action, exception.Message);
            return false;
        }
    }
}