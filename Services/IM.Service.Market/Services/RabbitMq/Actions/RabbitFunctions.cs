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

namespace IM.Service.Market.Services.RabbitMq.Actions;

public class RabbitFunctions : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctions(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateScope().ServiceProvider;

        return action switch
        {
            QueueActions.Create or QueueActions.Update or QueueActions.Delete => entity switch
            {
                QueueEntities.Price => Task.WhenAll(
                    serviceProvider.GetRequiredService<CoefficientService>().SetCoefficientAsync<Price>(data, action),
                    serviceProvider.GetRequiredService<PriceService>().SetValueTrueAsync<Price>(data, action)),
                QueueEntities.Prices => Task.WhenAll(
                    serviceProvider.GetRequiredService<CoefficientService>().SetCoefficientRangeAsync<Price>(data, action),
                    serviceProvider.GetRequiredService<PriceService>().SetValueTrueRangeAsync<Price>(data, action)),
                QueueEntities.Report => serviceProvider.GetRequiredService<CoefficientService>().SetCoefficientAsync<Report>(data, action),
                QueueEntities.Reports => serviceProvider.GetRequiredService<CoefficientService>().SetCoefficientRangeAsync<Report>(data, action),
                QueueEntities.Float => serviceProvider.GetRequiredService<CoefficientService>().SetCoefficientAsync<Float>(data, action),
                QueueEntities.Floats => serviceProvider.GetRequiredService<CoefficientService>().SetCoefficientRangeAsync<Float>(data, action),
                QueueEntities.Split => serviceProvider.GetRequiredService<PriceService>().SetValueTrueAsync<Split>(data, action),
                QueueEntities.Splits => serviceProvider.GetRequiredService<PriceService>().SetValueTrueRangeAsync<Split>(data, action),
                _ => Task.CompletedTask
            },
            QueueActions.Get => entity switch
            {
                QueueEntities.CompanySource => Task.WhenAll(
                    serviceProvider.GetRequiredService<PriceLoader>().LoadDataAsync(data),
                    serviceProvider.GetRequiredService<ReportLoader>().LoadDataAsync(data),
                    serviceProvider.GetRequiredService<FloatLoader>().LoadDataAsync(data),
                    serviceProvider.GetRequiredService<SplitLoader>().LoadDataAsync(data),
                    serviceProvider.GetRequiredService<DividendLoader>().LoadDataAsync(data)),
                QueueEntities.CompanySources => Task.WhenAll(
                    serviceProvider.GetRequiredService<PriceLoader>().LoadDataRangeAsync(data),
                    serviceProvider.GetRequiredService<ReportLoader>().LoadDataRangeAsync(data),
                    serviceProvider.GetRequiredService<FloatLoader>().LoadDataRangeAsync(data),
                    serviceProvider.GetRequiredService<SplitLoader>().LoadDataRangeAsync(data),
                    serviceProvider.GetRequiredService<DividendLoader>().LoadDataRangeAsync(data)),
                QueueEntities.Price => serviceProvider.GetRequiredService<PriceLoader>().LoadDataAsync(data),
                QueueEntities.Prices => serviceProvider.GetRequiredService<PriceLoader>().LoadDataRangeAsync(data),
                QueueEntities.Report => serviceProvider.GetRequiredService<ReportLoader>().LoadDataAsync(data),
                QueueEntities.Reports => serviceProvider.GetRequiredService<ReportLoader>().LoadDataRangeAsync(data),
                QueueEntities.Float => serviceProvider.GetRequiredService<FloatLoader>().LoadDataAsync(data),
                QueueEntities.Floats => serviceProvider.GetRequiredService<FloatLoader>().LoadDataRangeAsync(data),
                QueueEntities.Split => serviceProvider.GetRequiredService<SplitLoader>().LoadDataAsync(data),
                QueueEntities.Splits => serviceProvider.GetRequiredService<SplitLoader>().LoadDataRangeAsync(data),
                QueueEntities.Dividend => serviceProvider.GetRequiredService<DividendLoader>().LoadDataAsync(data),
                QueueEntities.Dividends => serviceProvider.GetRequiredService<DividendLoader>().LoadDataRangeAsync(data),
                _ => Task.CompletedTask
            },
            QueueActions.Set => entity switch
            {
                QueueEntities.Report => serviceProvider.GetRequiredService<ReportService>().ChangeStatusAsync(data, (byte)Statuses.Ready),
                QueueEntities.Reports => serviceProvider.GetRequiredService<ReportService>().ChangeStatusRangeAsync(data, (byte)Statuses.Ready),
                QueueEntities.Price => serviceProvider.GetRequiredService<PriceService>().ChangeStatusAsync(data, (byte)Statuses.Ready),
                QueueEntities.Prices => serviceProvider.GetRequiredService<PriceService>().ChangeStatusRangeAsync(data, (byte)Statuses.Ready),
                QueueEntities.Coefficient => serviceProvider.GetRequiredService<CoefficientService>().ChangeStatusAsync(data, (byte)Statuses.Ready),
                QueueEntities.Coefficients => serviceProvider.GetRequiredService<CoefficientService>().ChangeStatusRangeAsync(data, (byte)Statuses.Ready),
                _ => Task.CompletedTask
            },
            QueueActions.Compute => entity switch
            {
                QueueEntities.Rating => serviceProvider.GetRequiredService<RatingCalculator>().ComputeRatingAsync(),
                QueueEntities.Ratings => serviceProvider.GetRequiredService<RatingCalculator>().ComputeRatingAsync(),
                _ => Task.CompletedTask
            },
            _ => Task.CompletedTask
        };
    }
}