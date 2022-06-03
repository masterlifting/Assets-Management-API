using IM.Service.Shared.RabbitMq;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Services.Entity;

using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.RabbitMq.Function.Processes;

public sealed class ReportProcess : IRabbitProcess
{
    private readonly ReportService service;
    public ReportProcess(ReportService service) => this.service = service;

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Set => model switch
        {
            Report report => service.SetStatusAsync(report, Statuses.Ready),
            _ => Task.CompletedTask
        },
        QueueActions.Get => model switch
        {
            CompanySource companySource => service.Loader.LoadAsync(companySource),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Set => models switch
        {
            Report[] reports => service.SetStatusRangeAsync(reports, Statuses.Ready),
            _ => Task.CompletedTask
        },
        QueueActions.Get => models switch
        {
            CompanySource[] companySources => service.Loader.LoadAsync(companySources),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
}