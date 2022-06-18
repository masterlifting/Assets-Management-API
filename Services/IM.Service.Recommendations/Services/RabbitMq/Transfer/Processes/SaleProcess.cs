using IM.Service.Recommendations.Services.Entity;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;

using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Recommendations.Domain.Entities;

namespace IM.Service.Recommendations.Services.RabbitMq.Transfer.Processes;

public class SaleProcess : IRabbitProcess
{
    private readonly SaleService saleService;
    private readonly CompanyService companyService;

    public SaleProcess(SaleService saleService, CompanyService companyService)
    {
        this.saleService = saleService;
        this.companyService = companyService;
    }

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update => model switch
        {
            DealMqDto deal => companyService.SetCompanyAsync(deal).ContinueWith(x => saleService.SetAsync(x.Result)),
            _ => Task.CompletedTask
        },
        QueueActions.Delete => model switch
        {
            DealMqDto deal => companyService.SetCompanyAsync(deal).ContinueWith(x => saleService.DeleteAsync(x.Result)),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update => models switch
        {
            DealMqDto[] deals => companyService.SetCompaniesAsync(deals).ContinueWith(x => saleService.SetAsync(x.Result)),
            Company[] companies => saleService.SetAsync(companies),

            _ => Task.CompletedTask
        },
        QueueActions.Delete => models switch
        {
            DealMqDto[] deals => companyService.SetCompaniesAsync(deals).ContinueWith(x => saleService.DeleteAsync(x.Result)),
            Company[] companies => saleService.DeleteAsync(companies),

            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
}