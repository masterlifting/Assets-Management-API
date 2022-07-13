using IM.Service.Shared.Helpers;
using IM.Service.Shared.RabbitMq;
using IM.Service.Recommendations.Services.RabbitMq.Transfer.Processes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Recommendations.Services.Entity;
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
            QueueEntities.Ratings => Task.Run(async () =>
            {
                var ratings = JsonHelper.Deserialize<RatingMqDto[]>(data);
                var assets = await serviceProvider.GetRequiredService<AssetService>().SetAsync(ratings);

                await serviceProvider.GetRequiredService<SaleProcess>().ProcessRangeAsync(action, assets);
                await serviceProvider.GetRequiredService<PurchaseProcess>().ProcessRangeAsync(action, assets);
            }),
            QueueEntities.Deal => Task.Run(() =>
            {
                var deal = JsonHelper.Deserialize<DealMqDto>(data);
                return serviceProvider.GetRequiredService<SaleProcess>().ProcessAsync(action, deal);
            }),
            QueueEntities.Deals => Task.Run(() =>
            {
                var deals = JsonHelper.Deserialize<DealMqDto[]>(data);
                return serviceProvider.GetRequiredService<SaleProcess>().ProcessRangeAsync(action, deals);
            }),
            QueueEntities.Cost => Task.Run(() =>
            {
                var cost = JsonHelper.Deserialize<CostMqDto>(data);
                return serviceProvider.GetRequiredService<AssetProcess>().ProcessAsync(action, cost);
            }),
            QueueEntities.Costs => Task.Run(() =>
            {
                var costs = JsonHelper.Deserialize<CostMqDto[]>(data);

                return costs.Length == 1
                    ? serviceProvider.GetRequiredService<AssetProcess>().ProcessAsync(action, costs[0])
                    : serviceProvider.GetRequiredService<AssetProcess>().ProcessRangeAsync(action, costs);
            }),
            _ => Task.CompletedTask
        };
    }
}