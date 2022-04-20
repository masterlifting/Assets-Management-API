using IM.Service.Common.Net;
using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Portfolio.Models.Dto.Mq;
using IM.Service.Portfolio.Services.DataServices.Reports;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.MqServices.Implementations;

public class RabbitFunctionService : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctionService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        if (action != QueueActions.Get)
            return true;

        try
        {
            switch (entity)
            {
                case QueueEntities.Report:
                    {
                        if (!JsonHelper.TryDeserialize(data, out ReportFileDto? dto))
                            throw new SerializationException(nameof(ReportFileDto));

                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ReportLoader>();
                        await reportLoader.DataSetAsync(dto!);
                        break;
                    }
            }

            return true;
        }
        catch (Exception exception)
        {
            var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<RabbitFunctionService>>();
            logger.LogError(LogEvents.Function, "Entity: {entity} Queue action: {action} failed! Error: {error}", Enum.GetName(entity), action, exception.Message);
            return false;
        }
    }
}