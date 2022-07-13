using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Services.Entity;
using IM.Service.Shared.Helpers;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.RabbitMq.Function.Processes;

public sealed class DerivativeProcess : IRabbitProcess
{
    private readonly DerivativeService service;
    public DerivativeProcess(DerivativeService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Compute => model switch
        {
            Derivative derivative => service.ComputeToTransferAsync(derivative),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}"),
        },
        _ => service.Logger.LogDefaultTask($"{action}")
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Compute => models switch
        {
            Derivative[] derivatives => service.ComputeToTransferAsync(derivatives),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}s"),
        },
        _ => service.Logger.LogDefaultTask($"{action}")
    };
}