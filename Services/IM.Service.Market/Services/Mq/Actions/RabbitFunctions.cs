using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Calculations;
using IM.Service.Market.Services.DataLoaders.Dividends;
using IM.Service.Market.Services.DataLoaders.Floats;
using IM.Service.Market.Services.DataLoaders.Prices;
using IM.Service.Market.Services.DataLoaders.Reports;
using IM.Service.Market.Services.DataLoaders.Splits;

using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Mq.Actions;

public class RabbitFunctions : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctions(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        using var scope = scopeFactory.CreateScope();

        return action switch
        {
            QueueActions.Create or QueueActions.Update or QueueActions.Delete => entity switch
            {
                QueueEntities.Price => Task.WhenAll(
                    scope.ServiceProvider.GetRequiredService<CoefficientService>().SetCoefficientAsync<Price>(data, action),
                    scope.ServiceProvider.GetRequiredService<PriceService>().SetValueTrueAsync<Price>(data, action)),
                QueueEntities.Prices => Task.WhenAll(
                    scope.ServiceProvider.GetRequiredService<CoefficientService>().SetCoefficientRangeAsync<Price>(data, action),
                    scope.ServiceProvider.GetRequiredService<PriceService>().SetValueTrueRangeAsync<Price>(data, action)),
                QueueEntities.Report => scope.ServiceProvider.GetRequiredService<CoefficientService>().SetCoefficientAsync<Report>(data, action),
                QueueEntities.Reports => scope.ServiceProvider.GetRequiredService<CoefficientService>().SetCoefficientRangeAsync<Report>(data, action),
                QueueEntities.Float => scope.ServiceProvider.GetRequiredService<CoefficientService>().SetCoefficientAsync<Float>(data, action),
                QueueEntities.Floats => scope.ServiceProvider.GetRequiredService<CoefficientService>().SetCoefficientRangeAsync<Float>(data, action),
                QueueEntities.Split => scope.ServiceProvider.GetRequiredService<PriceService>().SetValueTrueAsync<Split>(data, action),
                QueueEntities.Splits => scope.ServiceProvider.GetRequiredService<PriceService>().SetValueTrueRangeAsync<Split>(data, action),
                _ => Task.CompletedTask
            },
            QueueActions.Get => entity switch
            {
                QueueEntities.CompanySource => Task.WhenAll(
                    scope.ServiceProvider.GetRequiredService<PriceLoader>().LoadDataAsync(data),
                    scope.ServiceProvider.GetRequiredService<ReportLoader>().LoadDataAsync(data),
                    scope.ServiceProvider.GetRequiredService<FloatLoader>().LoadDataAsync(data),
                    scope.ServiceProvider.GetRequiredService<SplitLoader>().LoadDataAsync(data),
                    scope.ServiceProvider.GetRequiredService<DividendLoader>().LoadDataAsync(data)),
                QueueEntities.CompanySources => Task.WhenAll(
                    scope.ServiceProvider.GetRequiredService<PriceLoader>().LoadDataRangeAsync(data),
                    scope.ServiceProvider.GetRequiredService<ReportLoader>().LoadDataRangeAsync(data),
                    scope.ServiceProvider.GetRequiredService<FloatLoader>().LoadDataRangeAsync(data),
                    scope.ServiceProvider.GetRequiredService<SplitLoader>().LoadDataRangeAsync(data),
                    scope.ServiceProvider.GetRequiredService<DividendLoader>().LoadDataRangeAsync(data)),
                QueueEntities.Price => scope.ServiceProvider.GetRequiredService<PriceLoader>().LoadDataAsync(data),
                QueueEntities.Prices => scope.ServiceProvider.GetRequiredService<PriceLoader>().LoadDataRangeAsync(data),
                QueueEntities.Report => scope.ServiceProvider.GetRequiredService<ReportLoader>().LoadDataAsync(data),
                QueueEntities.Reports => scope.ServiceProvider.GetRequiredService<ReportLoader>().LoadDataRangeAsync(data),
                QueueEntities.Float => scope.ServiceProvider.GetRequiredService<FloatLoader>().LoadDataAsync(data),
                QueueEntities.Floats => scope.ServiceProvider.GetRequiredService<FloatLoader>().LoadDataRangeAsync(data),
                QueueEntities.Split => scope.ServiceProvider.GetRequiredService<SplitLoader>().LoadDataAsync(data),
                QueueEntities.Splits => scope.ServiceProvider.GetRequiredService<SplitLoader>().LoadDataRangeAsync(data),
                QueueEntities.Dividend => scope.ServiceProvider.GetRequiredService<DividendLoader>().LoadDataAsync(data),
                QueueEntities.Dividends => scope.ServiceProvider.GetRequiredService<DividendLoader>().LoadDataRangeAsync(data),
                _ => Task.CompletedTask
            },
            QueueActions.Set => entity switch
            {
                QueueEntities.Report => scope.ServiceProvider.GetRequiredService<ReportService>().SetStatusAsync(data, (byte)Statuses.Ready),
                QueueEntities.Reports => scope.ServiceProvider.GetRequiredService<ReportService>().SetStatusRangeAsync(data, (byte)Statuses.Ready),
                _ => Task.CompletedTask
            },
            QueueActions.Compute => entity switch
            {
                QueueEntities.Ratings => scope.ServiceProvider.GetRequiredService<RatingCalculator>().ComputeRatingAsync(),
                _ => Task.CompletedTask
            },
            _ => Task.CompletedTask
        };
    }
}