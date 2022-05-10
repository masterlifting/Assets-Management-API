using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data.Dividends;
using IM.Service.Market.Services.Data.Floats;
using IM.Service.Market.Services.Data.Prices;
using IM.Service.Market.Services.Data.Reports;
using IM.Service.Market.Services.Data.Splits;
using IM.Service.Market.Services.Entity;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.RabbitMq.Actions;

public class RabbitFunctions : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctions(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateAsyncScope().ServiceProvider;

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
                    serviceProvider.GetRequiredService<PriceLoader>().LoadAsync(data),
                    serviceProvider.GetRequiredService<ReportLoader>().LoadAsync(data),
                    serviceProvider.GetRequiredService<FloatLoader>().LoadAsync(data),
                    serviceProvider.GetRequiredService<SplitLoader>().LoadAsync(data),
                    serviceProvider.GetRequiredService<DividendLoader>().LoadAsync(data)),
                QueueEntities.CompanySources => Task.WhenAll(
                    serviceProvider.GetRequiredService<PriceLoader>().LoadRangeAsync(data),
                    serviceProvider.GetRequiredService<ReportLoader>().LoadRangeAsync(data),
                    serviceProvider.GetRequiredService<FloatLoader>().LoadRangeAsync(data),
                    serviceProvider.GetRequiredService<SplitLoader>().LoadRangeAsync(data),
                    serviceProvider.GetRequiredService<DividendLoader>().LoadRangeAsync(data)),
                QueueEntities.Price => serviceProvider.GetRequiredService<PriceLoader>().LoadAsync(data),
                QueueEntities.Prices => serviceProvider.GetRequiredService<PriceLoader>().LoadRangeAsync(data),
                QueueEntities.Report => serviceProvider.GetRequiredService<ReportLoader>().LoadAsync(data),
                QueueEntities.Reports => serviceProvider.GetRequiredService<ReportLoader>().LoadRangeAsync(data),
                QueueEntities.Float => serviceProvider.GetRequiredService<FloatLoader>().LoadAsync(data),
                QueueEntities.Floats => serviceProvider.GetRequiredService<FloatLoader>().LoadRangeAsync(data),
                QueueEntities.Split => serviceProvider.GetRequiredService<SplitLoader>().LoadAsync(data),
                QueueEntities.Splits => serviceProvider.GetRequiredService<SplitLoader>().LoadRangeAsync(data),
                QueueEntities.Dividend => serviceProvider.GetRequiredService<DividendLoader>().LoadAsync(data),
                QueueEntities.Dividends => serviceProvider.GetRequiredService<DividendLoader>().LoadRangeAsync(data),
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