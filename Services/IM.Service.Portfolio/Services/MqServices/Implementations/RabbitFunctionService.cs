using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Portfolio.Services.DataServices.Reports;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.MqServices.Implementations;

public class RabbitFunctionService : IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunctionService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        if (action != QueueActions.Get)
            return Task.CompletedTask;

        using var scope = scopeFactory.CreateScope();

        return entity switch
        {
            QueueEntities.Report => scope.ServiceProvider.GetRequiredService<ReportLoader>().DataSetAsync(data),
            _ => Task.CompletedTask
        };
    }
}