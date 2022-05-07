using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Portfolio.Services.Data.Isins;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.RabbitMq.Actions;

public class RabbitSync : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitSync(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateAsyncScope().ServiceProvider;

        return action switch
        {
            QueueActions.Create or QueueActions.Update or QueueActions.Set => entity switch
            {
                QueueEntities.Company => serviceProvider.GetRequiredService<MoexIsinService>().GetAsync(data),
                QueueEntities.Companies => serviceProvider.GetRequiredService<MoexIsinService>().GetRangeAsync(data),
                _ => Task.CompletedTask
            },
            _ => Task.CompletedTask
        };
    }
}