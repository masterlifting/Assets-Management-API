using IM.Service.Shared.RabbitMq;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;
using IM.Service.Shared.Helpers;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Models.Api.Mq;
using IM.Service.Portfolio.Services.RabbitMq.Function.Processes;
using Microsoft.Extensions.Logging;

namespace IM.Service.Portfolio.Services.RabbitMq.Function;

public class RabbitFunction : IRabbitAction
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitFunction(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetResultAsync(QueueEntities entity, QueueActions action, string data)
    {
        var serviceProvider = scopeFactory.CreateAsyncScope().ServiceProvider;

        return entity switch
        {
            QueueEntities.Deal => serviceProvider.GetRequiredService<DealProcess>().ProcessAsync(action, JsonHelper.Deserialize<Deal>(data)),
            QueueEntities.Deals => serviceProvider.GetRequiredService<DealProcess>().ProcessRangeAsync(action, JsonHelper.Deserialize<Deal[]>(data)),
            QueueEntities.Event => serviceProvider.GetRequiredService<EventProcess>().ProcessAsync(action, JsonHelper.Deserialize<Event>(data)),
            QueueEntities.Events => serviceProvider.GetRequiredService<EventProcess>().ProcessRangeAsync(action, JsonHelper.Deserialize<Event[]>(data)),
            QueueEntities.Derivative => serviceProvider.GetRequiredService<DerivativeProcess>().ProcessAsync(action, JsonHelper.Deserialize<Derivative>(data)),
            QueueEntities.Derivatives => serviceProvider.GetRequiredService<DerivativeProcess>().ProcessRangeAsync(action, JsonHelper.Deserialize<Derivative[]>(data)),
            QueueEntities.Report => serviceProvider.GetRequiredService<ReportProcess>().ProcessAsync(action, JsonHelper.Deserialize<ProviderReportDto>(data)),
            _ => serviceProvider.GetRequiredService<ILogger<RabbitFunction>>().LogDefaultTask($"{action} {entity}")
        };
    }
}