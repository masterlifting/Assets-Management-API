using System.Runtime.Serialization;

using IM.Service.Common.Net;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Services.Calculations;
using IM.Service.Market.Services.DataLoaders;

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
                            QueueEntities.CompanySource => LoadDataAsync(data, false),
                            QueueEntities.CompanySources => LoadDataAsync(data, true),
                            QueueEntities.Float => LoadDataAsync<Float>(data, false),
                            QueueEntities.Floats => LoadDataAsync<Float>(data, true),
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

    private Task LoadDataAsync(string data, bool isCollection)
    {
        var serviceProvider = scopeFactory.CreateScope().ServiceProvider;

        var priceLoader = serviceProvider.GetRequiredService<DataLoader<Price>>();
        var reportLoader = serviceProvider.GetRequiredService<DataLoader<Report>>();
        var floatLoader = serviceProvider.GetRequiredService<DataLoader<Float>>();
        var splitLoader = serviceProvider.GetRequiredService<DataLoader<Split>>();

        return isCollection
            ? RabbitHelper.TrySerialize(data, out CompanySource[]? companySources)
                ? SetDataAsync1(companySources!)
                : SetDataAsync3()
            : RabbitHelper.TrySerialize(data, out CompanySource? companySource)
                ? SetDataAsync2(companySource!)
                : throw new SerializationException(nameof(CompanySource));

        async Task SetDataAsync1(CompanySource[] css)
        {
            await Task.WhenAll(
                priceLoader.DataSetAsync(css),
                reportLoader.DataSetAsync(css),
                floatLoader.DataSetAsync(css),
                splitLoader.DataSetAsync(css));
        }
        async Task SetDataAsync2(CompanySource cs)
        {
            await Task.WhenAll(
                priceLoader.DataSetAsync(cs),
                reportLoader.DataSetAsync(cs),
                floatLoader.DataSetAsync(cs),
                splitLoader.DataSetAsync(cs));
        }
        async Task SetDataAsync3()
        {
            await Task.WhenAll(
                priceLoader.DataSetAsync(),
                reportLoader.DataSetAsync(),
                floatLoader.DataSetAsync(),
                splitLoader.DataSetAsync());
        }
    }
    private Task LoadDataAsync<T>(string data, bool isCollection) where T : class, IDataIdentity, IPeriod
    {
        var loader = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DataLoader<T>>();

        return isCollection
            ? RabbitHelper.TrySerialize(data, out CompanySource[]? entities)
                ? loader.DataSetAsync(entities!)
                : throw new SerializationException(typeof(T).Name)
            : RabbitHelper.TrySerialize(data, out CompanySource? entity)
                    ? loader.DataSetAsync(entity!)
                    : throw new SerializationException(typeof(T).Name);
    }
}