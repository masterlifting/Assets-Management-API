using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using IM.Service.Shared.RabbitMq;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Settings;

using Microsoft.Extensions.Options;
using IM.Service.Shared.Models.RabbitMq.Api;

namespace IM.Service.Portfolio.Services.RabbitMq.Function.Processes;

public sealed class DealProcess : IRabbitProcess
{
    private readonly Repository<UnderlyingAsset> underlyingAssetRepo;
    private readonly Repository<Derivative> derivativeRepo;
    private readonly Repository<Deal> dealRepo;
    private readonly string rabbitConnectionString;

    public DealProcess(IOptions<ServiceSettings> options, Repository<UnderlyingAsset> underlyingAssetRepo, Repository<Derivative> derivativeRepo, Repository<Deal> dealRepo)
    {
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        this.underlyingAssetRepo = underlyingAssetRepo;
        this.derivativeRepo = derivativeRepo;
        this.dealRepo = dealRepo;
    }

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Compute => model switch
        {
            Deal deal => SetAsync(deal),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Compute => models switch
        {
            Deal[] deals => SetAsync(deals),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };

    private Task SetAsync(Deal model) => TransferDealAsync(model);
    private Task SetAsync(IEnumerable<Deal> models) => TransferDealsAsync(models);

    private async Task TransferDealsAsync(IEnumerable<Deal> entities)
    {
        var models = await GetDealsToTransferAsync(entities);

        var computeResult = models.Where(x => x.SumValue > 0).ToArray();
        var deleteResult = models.Where(x => x.SumValue == 0).ToArray();

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        if (computeResult.Any())
            publisher.PublishTask(QueueNames.Recommendations, QueueEntities.Deals, QueueActions.Create, computeResult);
        if (deleteResult.Any())
            publisher.PublishTask(QueueNames.Recommendations, QueueEntities.Deals, QueueActions.Delete, deleteResult);
    }
    private async Task TransferDealAsync(Deal entity) => await TransferDealsAsync(new[] {entity});

    private async Task<DealMqDto[]> GetDealsToTransferAsync(IEnumerable<Deal> entities)
    {
        var derivativeIds = entities.Select(x => x.DerivativeId).Distinct();

        var underlyingAssetIds = await underlyingAssetRepo.GetSampleAsync(
            x => x.UnderlyingAssetTypeId == (byte)Enums.UnderlyingAssetTypes.Stock,
            x => x.Id);

        var derivatives = await derivativeRepo.GetSampleAsync(
            x => underlyingAssetIds.Contains(x.UnderlyingAssetId) && derivativeIds.Contains(x.Id),
            x => ValueTuple.Create(x.Id, x.UnderlyingAssetId));

        var deals = await dealRepo.GetSampleAsync(
            x => derivativeIds.Contains(x.DerivativeId),
            x => new { x.DerivativeId, x.OperationId, x.Cost, x.Value });

        return deals
            .Join(derivatives, x => x.DerivativeId, y => y.Item1, (x, y) => new
            {
                CompanyId = y.Item2,
                x.OperationId,
                x.Value,
                x.Cost
            })
            .GroupBy(x => x.CompanyId)
            .Select(x =>
            {
                var incomingDeals = x.Where(y => y.OperationId == (byte)Enums.OperationTypes.Приход).ToArray();
                var expenseDeals = x.Where(y => y.OperationId == (byte)Enums.OperationTypes.Расход).OrderByDescending(y => y.Cost).ToArray();

                var sumValue = expenseDeals.Sum(y => y.Value) - incomingDeals.Sum(y => y.Value);

                var sumCost = 0m;
                var _sumValue = 0m;

                if (sumValue <= 0)
                    return new DealMqDto(x.Key, sumValue, sumCost);

                foreach (var deal in expenseDeals)
                {
                    _sumValue += deal.Value;
                    var dealValue = deal.Value;

                    if (_sumValue > sumValue)
                    {
                        dealValue = _sumValue - deal.Value - sumValue;
                        _sumValue += dealValue;
                        dealValue = Math.Abs(dealValue);
                    }

                    sumCost += deal.Cost * dealValue;

                    if (sumValue == _sumValue)
                        break;
                }

                return new DealMqDto(x.Key, sumValue, sumCost);
            })
            .ToArray();
    }
}