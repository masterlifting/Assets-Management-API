using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Services.Entity;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Shared.Helpers;

namespace IM.Service.Portfolio.Services.RabbitMq.Function.Processes;

public sealed class EventProcess : IRabbitProcess
{
    private readonly EventService service;
    public EventProcess(EventService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Delete => model switch
        {
            Event _event => service.SetDerivativeBalanceAsync(action, _event),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}"),
        },
        _ => service.Logger.LogDefaultTask($"{action}")
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Delete => models switch
        {
            Event[] events => service.SetDerivativeBalancesAsync(action, events),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}s"),
        },
        _ => service.Logger.LogDefaultTask($"{action}")
    };
}