using IM.Service.Portfolio.Services.Entity;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Shared.Helpers;

namespace IM.Service.Portfolio.Services.RabbitMq.Sync.Processes;

public class AssetProcess : IRabbitProcess
{
    private readonly AssetService service;
    public AssetProcess(AssetService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Set => model switch
        {
            AssetMqDto asset => service.SetAsync(asset),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}"),
        },
        _ => service.Logger.LogDefaultTask($"{action}")
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Set => models switch
        {
            AssetMqDto[] companies => service.SetAsync(companies),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}s"),
        },
        _ => service.Logger.LogDefaultTask($"{action}"),
    };
}