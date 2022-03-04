using IM.Service.Common.Net;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Data.Services.DataFounders.Floats;
using IM.Service.Data.Services.DataFounders.Prices;
using IM.Service.Data.Services.DataFounders.Reports;
using IM.Service.Data.Services.DataFounders.Splits;

namespace IM.Service.Data.Services.Mq.Actions;

public class RabbitFunctions : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctions(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string companyId)
    {
        if (action != QueueActions.Call)
            return true;

        try
        {
            switch (entity)
            {
                case QueueEntities.Price:
                    {
                        var priceLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<PriceLoader>();
                        await priceLoader.DataSetAsync(companyId);
                        break;
                    }
                case QueueEntities.Prices:
                    {
                        var priceLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<PriceLoader>();
                        await priceLoader.DataSetAsync();
                        break;
                    }
                case QueueEntities.Report:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ReportLoader>();
                        await reportLoader.DataSetAsync(companyId);
                        break;
                    }
                case QueueEntities.Reports:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ReportLoader>();
                        await reportLoader.DataSetAsync();
                        break;
                    }
                case QueueEntities.Split:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<StockSplitLoader>();
                        await reportLoader.DataSetAsync(companyId);
                        break;
                    }
                case QueueEntities.Splits:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<StockSplitLoader>();
                        await reportLoader.DataSetAsync();
                        break;
                    }
                case QueueEntities.StockVolume:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<StockVolumeLoader>();
                        await reportLoader.DataSetAsync(companyId);
                        break;
                    }
                case QueueEntities.StockVolumes:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<StockVolumeLoader>();
                        await reportLoader.DataSetAsync();
                        break;
                    }
            }

            return true;
        }
        catch (Exception exception)
        {
            var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<RabbitFunctions>>();
            logger.LogError(LogEvents.Call, "Entity: {entity} Queue action: {action} failed! \nError: {error}", Enum.GetName(entity), action, exception.Message);
            return false;
        }
    }
}