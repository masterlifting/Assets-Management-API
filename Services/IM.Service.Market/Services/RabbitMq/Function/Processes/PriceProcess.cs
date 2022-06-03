using IM.Service.Shared.RabbitMq;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Services.Entity;

using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.RabbitMq.Function.Processes;

public sealed class PriceProcess : IRabbitProcess
{
    private readonly PriceService service;
    public PriceProcess(PriceService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Set => model switch
        {
            Price price => service.SetStatusAsync(price, Statuses.Ready),
            _ => Task.CompletedTask
        },
        QueueActions.Get => model switch
        {
            CompanySource companySource => service.Loader.LoadAsync(companySource),
            _ => Task.CompletedTask
        },
        QueueActions.Create or QueueActions.Update or QueueActions.Delete => model switch
        {
            Price price => service.SetValueTrueAsync(action, price),
            Split split => service.SetValueTrueAsync(action, split),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Set => models switch
        {
            Price[] prices => service.SetStatusRangeAsync(prices, Statuses.Ready),
            _ => Task.CompletedTask
        },
        QueueActions.Get => models switch
        {
            CompanySource[] companySources => service.Loader.LoadAsync(companySources),
            _ => Task.CompletedTask
        },
        QueueActions.Create or QueueActions.Update or QueueActions.Delete => models switch
        {
            Price[] prices => service.SetValueTrueAsync(action, prices),
            Split[] splits => service.SetValueTrueAsync(action, splits),

            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
}