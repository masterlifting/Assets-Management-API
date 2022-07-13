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
    private readonly AssetService assetService;

    public SaleProcess(SaleService saleService, AssetService assetService)
    {
        this.saleService = saleService;
        this.assetService = assetService;
    }

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update => model switch
        {
            DealMqDto deal => assetService.SetAsync(deal).ContinueWith(x => saleService.SetAsync(x.Result)),
            _ => Task.CompletedTask
        },
        QueueActions.Delete => model switch
        {
            DealMqDto deal => assetService.SetAsync(deal).ContinueWith(x => saleService.DeleteAsync(x.Result)),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update => models switch
        {
            DealMqDto[] deals => assetService.SetAsync(deals).ContinueWith(x => saleService.SetAsync(x.Result)),
            Asset[] assets => saleService.SetAsync(assets),

            _ => Task.CompletedTask
        },
        QueueActions.Delete => models switch
        {
            DealMqDto[] deals => assetService.SetAsync(deals).ContinueWith(x => saleService.DeleteAsync(x.Result)),
            Asset[] assets => saleService.DeleteAsync(assets),

            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
}