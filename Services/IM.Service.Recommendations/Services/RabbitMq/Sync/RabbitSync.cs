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
            QueueEntities.Company => serviceProvider.GetRequiredService<CompanyProcess>().ProcessAsync(action, JsonHelper.Deserialize<CompanyMqDto>(data)),
            QueueEntities.Companies => serviceProvider.GetRequiredService<CompanyProcess>().ProcessRangeAsync(action, JsonHelper.Deserialize<CompanyMqDto[]>(data)),
            _ => Task.CompletedTask
        };
    }
}