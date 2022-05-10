using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IM.Service.Recommendations.Services.RabbitMq.Actions;

public class RabbitTransfer : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitTransfer(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateAsyncScope().ServiceProvider;

        return action switch
        {
            QueueActions.Create or QueueActions.Update or QueueActions.Delete => entity switch
            {
                //QueueEntities.Deal => serviceProvider.GetRequiredService<DealService>().SetSummaryAsync(data, action),
                //QueueEntities.Deals => serviceProvider.GetRequiredService<DealService>().SetSummaryRangeAsync(data, action),
                _ => Task.CompletedTask
            },
            QueueActions.Get => entity switch
            {
                //QueueEntities.Report => serviceProvider.GetRequiredService<ReportLoader>().LoadAsync(data),
                _ => Task.CompletedTask
            },
            _ => Task.CompletedTask
        };
    }
}