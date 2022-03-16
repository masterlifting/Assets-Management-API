using System.Runtime.Serialization;

using IM.Service.Common.Net;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using IM.Service.MarketData.Domain.Entities.ManyToMany;
using IM.Service.MarketData.Services.Calculations;
using IM.Service.MarketData.Services.DataLoaders;

namespace IM.Service.MarketData.Services.Mq.Actions;

public class RabbitFunctions : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctions(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
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

                            _ => Task.CompletedTask
                        };

                        await loadTask;
                        break;
                    }
                case QueueActions.Compute:
                    {
                        var computeTask = entity switch
                        {
                            QueueEntities.Report => SetCoefficientAsync<Report>(data, false),
                            QueueEntities.Reports => SetCoefficientAsync<Report>(data, true),
                            QueueEntities.Float => SetCoefficientAsync<Float>(data, false),
                            QueueEntities.Floats => SetCoefficientAsync<Float>(data, true),
                            QueueEntities.Price => SetCoefficientAsync<Price>(data, false),
                            QueueEntities.Prices => SetCoefficientAsync<Price>(data, true),
                            QueueEntities.Split => SetPriceAsync(data, false),
                            QueueEntities.Splits => SetPriceAsync(data, true),
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
    private Task SetCoefficientAsync<T>(string data, bool isCollection) where T : class, IDataIdentity
    {
        var service = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<CoefficientService>();

        return isCollection ? service.SetCollectionAsync<T>(data) : service.SetAsync<T>(data);
    }
    private Task SetPriceAsync(string data, bool isCollection)
    {
        var service = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<PriceService>();

        return isCollection ? service.SetValueTrueCollectionAsync(data) : service.SetValueTrueAsync(data);
    }
}