using IM.Service.Common.Net;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Company.Data.Services.DataServices.Prices;
using IM.Service.Company.Data.Services.DataServices.Reports;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Company.Data.Services.DataServices.StockSplits;
using IM.Service.Company.Data.Services.DataServices.StockVolumes;

namespace IM.Service.Company.Data.Services.MqServices.Implementations;

public class RabbitFunctionService : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctionService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

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
                case QueueEntities.CompanyReport:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ReportLoader>();
                        await reportLoader.DataSetAsync(companyId);
                        break;
                    }
                case QueueEntities.CompanyReports:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ReportLoader>();
                        await reportLoader.DataSetAsync();
                        break;
                    }
                case QueueEntities.StockSplit:
                    {
                        var reportLoader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<StockSplitLoader>();
                        await reportLoader.DataSetAsync(companyId);
                        break;
                    }
                case QueueEntities.StockSplits:
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
            var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<RabbitFunctionService>>();
            logger.LogError(LogEvents.Call, "Entity: {entity} Queue action: {action} failed! \nError: {error}", Enum.GetName(entity), action, exception.Message);
            return false;
        }
    }
}