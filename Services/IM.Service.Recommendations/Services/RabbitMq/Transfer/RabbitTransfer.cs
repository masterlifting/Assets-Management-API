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
                var companies = await serviceProvider.GetRequiredService<CompanyService>().SetCompaniesAsync(ratings);
                return TaskHelper.WhenAny(methodName, logger, new List<Task>
                {
                    serviceProvider.GetRequiredService<SaleProcess>().ProcessRangeAsync(action, companies),
                    serviceProvider.GetRequiredService<PurchaseProcess>().ProcessRangeAsync(action, companies)
                });
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
            QueueEntities.Price => Task.Run(() =>
            {
                var price = JsonHelper.Deserialize<PriceMqDto>(data);
                return serviceProvider.GetRequiredService<CompanyProcess>().ProcessAsync(action, price);
            }),
            QueueEntities.Prices => Task.Run(() =>
            {
                var prices = JsonHelper.Deserialize<PriceMqDto[]>(data);

                return prices.Length == 1
                    ? serviceProvider.GetRequiredService<CompanyProcess>().ProcessAsync(action, prices[0])
                    : serviceProvider.GetRequiredService<CompanyProcess>().ProcessRangeAsync(action, prices);
            }),
            _ => Task.CompletedTask
        };
    }
}