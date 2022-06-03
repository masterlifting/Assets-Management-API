using IM.Service.Recommendations.Services.Entity;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.RabbitMq.Transfer.Processes;

public class PurchaseProcess : IRabbitProcess
{
    private readonly PurchaseService service;
    public PurchaseProcess(PurchaseService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => Task.CompletedTask;
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update => models switch
        {
            RatingMqDto[] ratings => service.SetAsync(ratings),

            _ => Task.CompletedTask
        },
        QueueActions.Delete => models switch
        {
            RatingMqDto[] ratings => service.DeleteAsync(ratings),
            _ => Task.CompletedTask

        },
        _ => Task.CompletedTask
    };
}