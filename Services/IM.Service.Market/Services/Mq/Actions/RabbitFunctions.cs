using IM.Service.Common.Net;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Calculations;
using IM.Service.Market.Services.DataLoaders.Floats;
using IM.Service.Market.Services.DataLoaders.Prices;
using IM.Service.Market.Services.DataLoaders.Reports;
using IM.Service.Market.Services.DataLoaders.Splits;

namespace IM.Service.Market.Services.Mq.Actions;

public class RabbitFunctions : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctions(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateScope().ServiceProvider;

        try
        {
            switch (action)
            {
                case QueueActions.Get:
                    {
                        var loadTask = entity switch
                        {
                            QueueEntities.CompanySource => Task.WhenAll(
                                serviceProvider.GetRequiredService<PriceLoader>().LoadDataAsync(data),
                                serviceProvider.GetRequiredService<ReportLoader>().LoadDataAsync(data),
                                serviceProvider.GetRequiredService<FloatLoader>().LoadDataAsync(data),
                                serviceProvider.GetRequiredService<SplitLoader>().LoadDataAsync(data)),
                            QueueEntities.CompanySources => Task.WhenAll(
                                serviceProvider.GetRequiredService<PriceLoader>().LoadRangeDataAsync(data),
                                serviceProvider.GetRequiredService<ReportLoader>().LoadRangeDataAsync(data),
                                serviceProvider.GetRequiredService<FloatLoader>().LoadRangeDataAsync(data),
                                serviceProvider.GetRequiredService<SplitLoader>().LoadRangeDataAsync(data)),
                            QueueEntities.Price => serviceProvider.GetRequiredService<PriceLoader>().LoadDataAsync(data),
                            QueueEntities.Prices => serviceProvider.GetRequiredService<PriceLoader>().LoadRangeDataAsync(data),
                            QueueEntities.Report => serviceProvider.GetRequiredService<ReportLoader>().LoadDataAsync(data),
                            QueueEntities.Reports => serviceProvider.GetRequiredService<ReportLoader>().LoadRangeDataAsync(data),
                            QueueEntities.Float => serviceProvider.GetRequiredService<FloatLoader>().LoadDataAsync(data),
                            QueueEntities.Floats => serviceProvider.GetRequiredService<FloatLoader>().LoadRangeDataAsync(data),
                            QueueEntities.Split => serviceProvider.GetRequiredService<SplitLoader>().LoadDataAsync(data),
                            QueueEntities.Splits => serviceProvider.GetRequiredService<SplitLoader>().LoadRangeDataAsync(data),
                            _ => Task.CompletedTask
                        };

                        await loadTask;
                        break;
                    }
                case QueueActions.Compute:
                    {
                        var computeTask = entity switch
                        {
                            QueueEntities.Report => serviceProvider.GetRequiredService<CoefficientService>().SetAsync<Report>(data),
                            QueueEntities.Reports => serviceProvider.GetRequiredService<CoefficientService>().SetRangeAsync<Report>(data),
                            QueueEntities.Float => serviceProvider.GetRequiredService<CoefficientService>().SetAsync<Float>(data),
                            QueueEntities.Floats => serviceProvider.GetRequiredService<CoefficientService>().SetRangeAsync<Float>(data),
                            QueueEntities.Price => serviceProvider.GetRequiredService<CoefficientService>().SetAsync<Price>(data),
                            QueueEntities.Prices => serviceProvider.GetRequiredService<CoefficientService>().SetRangeAsync<Price>(data),
                            QueueEntities.Split => serviceProvider.GetRequiredService<PriceService>().SetValueTrueAsync(data),
                            QueueEntities.Splits => serviceProvider.GetRequiredService<PriceService>().SetValueTrueRangeAsync(data),
                            QueueEntities.Ratings => serviceProvider.GetRequiredService<RatingCalculator>().SetRatingAsync(),
                            _ => Task.CompletedTask
                        };

                        await computeTask;
                        break;
                    }
            }

            return true;
        }
        catch (Exception exception)
        {
            var logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<RabbitFunctions>>();
            logger.LogError(LogEvents.Function, "Entity: {entity} Queue action: {action} failed! \nError: {error}", Enum.GetName(entity), action, exception.Message);
            return false;
        }
    }
}