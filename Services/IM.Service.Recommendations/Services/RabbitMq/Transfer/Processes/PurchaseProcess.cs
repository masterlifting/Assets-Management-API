using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Services.Entity;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.RabbitMq.Transfer.Processes;

public class PurchaseProcess : IRabbitProcess
{
    private readonly PurchaseService purchaseService;
    public PurchaseProcess(PurchaseService purchaseService) => this.purchaseService = purchaseService;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => Task.CompletedTask;
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update => models switch
        {
            Asset[] companies => purchaseService.SetAsync(companies),
            _ => Task.CompletedTask
        },
        QueueActions.Delete => models switch
        {
            Asset[] companies => purchaseService.DeleteAsync(companies),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
}