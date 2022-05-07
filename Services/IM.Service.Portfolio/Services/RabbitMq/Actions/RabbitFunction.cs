using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Portfolio.Services.Data.Reports;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.RabbitMq.Actions;

public class RabbitFunction : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunction(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateAsyncScope().ServiceProvider;

        return action switch
        {
            QueueActions.Get => entity switch
            {
                QueueEntities.Report => serviceProvider.GetRequiredService<ReportLoader>().LoadAsync(data),
                _ => Task.CompletedTask
            },
            _ => Task.CompletedTask
        };
    }
}