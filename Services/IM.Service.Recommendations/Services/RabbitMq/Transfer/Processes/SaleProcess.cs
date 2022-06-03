using IM.Service.Recommendations.Services.Entity;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.RabbitMq.Transfer.Processes;

public class SaleProcess : IRabbitProcess
{
    private readonly SaleService service;
    public SaleProcess(SaleService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update => model switch
        {
            DealMqDto deal => service.SetAsync(deal),
            _ => Task.CompletedTask
        },
        QueueActions.Delete => model switch
        {
            DealMqDto deal => service.DeleteAsync(deal),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update => models switch
        {
            DealMqDto[] deals => service.SetAsync(deals),
            RatingMqDto[] ratings => service.SetAsync(ratings),
            _ => Task.CompletedTask
        },
        QueueActions.Delete => models switch
        {
            DealMqDto[] deals => service.DeleteAsync(deals),
            RatingMqDto[] ratings => service.DeleteAsync(ratings),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
}