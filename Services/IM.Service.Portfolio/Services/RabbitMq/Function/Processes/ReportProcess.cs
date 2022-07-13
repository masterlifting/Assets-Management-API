using IM.Service.Portfolio.Models.Api.Mq;
using IM.Service.Portfolio.Services.Entity;
using IM.Service.Shared.Helpers;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Portfolio.Services.RabbitMq.Function.Processes;

public sealed class ReportProcess : IRabbitProcess
{
    private readonly ReportService service;
    public ReportProcess(ReportService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Get => model switch
        {
            ProviderReportDto report => service.SetAsync(report),
            _ => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}"),
        },
        _ => service.Logger.LogDefaultTask($"{action}")
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class
        => service.Logger.LogDefaultTask($"{action} {typeof(T).Name}");
}