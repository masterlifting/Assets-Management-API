using IM.Service.Shared.RabbitMq;
using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;
using IM.Service.Shared.Helpers;
using IM.Service.Recommendations.Services.RabbitMq.Sync.Processes;
using IM.Service.Shared.Models.RabbitMq.Api;

namespace IM.Service.Recommendations.Services.RabbitMq.Sync;

public class RabbitSync : IRabbitAction
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitSync(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateAsyncScope().ServiceProvider;

        return entity switch
        {
            QueueEntities.Asset => serviceProvider.GetRequiredService<AssetProcess>().ProcessAsync(action, JsonHelper.Deserialize<AssetMqDto>(data)),
            QueueEntities.Assets => serviceProvider.GetRequiredService<AssetProcess>().ProcessRangeAsync(action, JsonHelper.Deserialize<AssetMqDto[]>(data)),
            _ => Task.CompletedTask
        };
    }
}