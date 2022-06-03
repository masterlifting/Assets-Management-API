using IM.Service.Shared.Helpers;
using IM.Service.Shared.RabbitMq;
using IM.Service.Recommendations.Services.RabbitMq.Transfer.Processes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Shared.Helpers.ServiceHelper;
using IM.Service.Shared.Models.RabbitMq.Api;

namespace IM.Service.Recommendations.Services.RabbitMq.Transfer;

public class RabbitTransfer : IRabbitAction
{
    private const string methodName = nameof(GetResultAsync);
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitTransfer(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateAsyncScope().ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<RabbitTransfer>>();

        return entity switch
        {
            QueueEntities.Ratings => Task.Run(() =>
            {
                var models = JsonHelper.Deserialize<RatingMqDto[]>(data);

                var companyProcessTask = serviceProvider.GetRequiredService<CompanyProcess>().ProcessRangeAsync(action, models);
                var recommendationsTask = TaskHelper.WhenAny(methodName, logger, new List<Task>
                {
                    serviceProvider.GetRequiredService<SaleProcess>().ProcessRangeAsync(action, models),
                    serviceProvider.GetRequiredService<PurchaseProcess>().ProcessRangeAsync(action, models)
                });

                return companyProcessTask.ContinueWith(_ => recommendationsTask);
            }),
            QueueEntities.Deal => Task.Run(() =>
            {
                var model = JsonHelper.Deserialize<DealMqDto>(data);

                var companyProcessTask = serviceProvider.GetRequiredService<CompanyProcess>().ProcessAsync(action, model);
                var recommendationsTask = serviceProvider.GetRequiredService<SaleProcess>().ProcessAsync(action, model);

                return companyProcessTask.ContinueWith(_ => recommendationsTask);
            }),
            QueueEntities.Deals => Task.Run(() =>
            {
                var models = JsonHelper.Deserialize<DealMqDto[]>(data);

                var companyProcessTask = serviceProvider.GetRequiredService<CompanyProcess>().ProcessRangeAsync(action, models);
                var recommendationsTask = serviceProvider.GetRequiredService<SaleProcess>().ProcessRangeAsync(action, models);

                return companyProcessTask.ContinueWith(_ => recommendationsTask);
            }),
            QueueEntities.Price => Task.Run(() =>
            {
                var model = JsonHelper.Deserialize<PriceMqDto>(data);
                return serviceProvider.GetRequiredService<CompanyProcess>().ProcessAsync(action, model);
            }),
            QueueEntities.Prices => Task.Run(() =>
            {
                var models = JsonHelper.Deserialize<PriceMqDto[]>(data);
                return serviceProvider.GetRequiredService<CompanyProcess>().ProcessRangeAsync(action, models);
            }),
            _ => Task.CompletedTask
        };
    }
}