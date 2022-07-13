using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Services.Entity;
using IM.Service.Shared.Helpers;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.RabbitMq.Function.Processes;

public sealed class DealProcess : IRabbitProcess
{
    private readonly DealService service;
    public DealProcess(DealService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Delete => model switch
        {
            Deal deal => service.SetAsync(action, deal),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}"),
        },
        _ => service.Logger.LogDefaultTask($"{action}")
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Delete => models switch
        {
            Deal[] deals => service.SetAsync(action, deals),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}s"),
        },
        _ => service.Logger.LogDefaultTask($"{action}")
    };
}