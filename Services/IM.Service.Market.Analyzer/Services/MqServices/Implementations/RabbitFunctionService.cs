using System;
using System.Threading.Tasks;

using IM.Service.Common.Net;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Analyzer.Services.CalculatorServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IM.Service.Market.Analyzer.Services.MqServices.Implementations;

public class RabbitFunctionService : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctionService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string info)
    {
        if (action != QueueActions.Call)
            return true;

        try
        {
            switch (entity)
            {
                case QueueEntities.Ratings:
                {
                    var ratingService = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<RatingService>();
                    await ratingService.SetRatingAsync();
                    break;
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