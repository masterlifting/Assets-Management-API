using IM.Service.Shared.RabbitMq;
using IM.Service.Recommendations.Services.Entity;

using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Shared.Models.RabbitMq.Api;

namespace IM.Service.Recommendations.Services.RabbitMq.Transfer.Processes;

public class AssetProcess : IRabbitProcess
{
    private readonly AssetService service;
    public AssetProcess(AssetService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Delete => model switch
        {
            CostMqDto cost => service.SetAsync(cost),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Delete => models switch
        {
            CostMqDto[] costs => service.SetAsync(costs),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
}