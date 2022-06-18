using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Services.Entity;
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
        QueueActions.Compute => model switch
        {
            Deal deal => service.ComputeSummaryAsync(deal),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Compute => models switch
        {
            Deal[] deals => service.ComputeSummariesAsync(deals),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };

}