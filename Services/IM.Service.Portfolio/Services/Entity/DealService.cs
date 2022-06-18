using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Models.Service;
using IM.Service.Portfolio.Settings;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;

using Microsoft.Extensions.Options;

using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.Entity;

public class DealService
{
    private readonly Repository<UnderlyingAsset> underlyingAssetRepo;
    private readonly Repository<Derivative> derivativeRepo;
    private readonly Repository<Deal> dealRepo;
    private readonly string rabbitConnectionString;

    public DealService(IOptions<ServiceSettings> options, Repository<UnderlyingAsset> underlyingAssetRepo, Repository<Derivative> derivativeRepo, Repository<Deal> dealRepo)
    {
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        this.underlyingAssetRepo = underlyingAssetRepo;
        this.derivativeRepo = derivativeRepo;
        this.dealRepo = dealRepo;
    }

    public async Task ComputeSummariesAsync(IEnumerable<Deal> entities)
    {
        var derivativeIds = entities.Select(x => x.DerivativeId).Distinct();
        var dealSummaries = await GetSummariesAsync(derivativeIds);

        var dealMqModels = new List<DealMqDto>(dealSummaries.Length);
        foreach (var (companyId, value, cost) in dealSummaries)
            if (value != 0)
            {
                if (cost >= 0)
                    dealMqModels.Add(new(companyId, value, cost));
            }
            else
                dealMqModels.Add(new(companyId, null, null));

        TransferToRecommendations(dealMqModels.ToArray());
    }
    public async Task ComputeSummaryAsync(Deal entity)
    {
        var dealSummaries = await GetSummariesAsync(new[] { entity.DerivativeId });
        var dealMqModels = new List<DealMqDto>(dealSummaries.Length);
        foreach (var (companyId, value, cost) in dealSummaries)
            if (value != 0)
            {
                if (cost >= 0)
                    dealMqModels.Add(new(companyId, value, cost));
            }
            else
                dealMqModels.Add(new(companyId, null, null));

        TransferToRecommendations(dealMqModels.ToArray());
    }

    private async Task<DealSummary[]> GetSummariesAsync(IEnumerable<string> derivativeIds)
    {
        var underlyingAssetIds = await underlyingAssetRepo.GetSampleAsync(
            x => x.UnderlyingAssetTypeId == (byte)UnderlyingAssetTypes.Stock,
            x => x.Id);

        var derivatives = await derivativeRepo.GetSampleAsync(
            x => underlyingAssetIds.Contains(x.UnderlyingAssetId) && derivativeIds.Contains(x.Id),
            x => ValueTuple.Create(x.Id, x.UnderlyingAssetId));

        derivatives = derivatives
            .GroupBy(x => x.Item1)
            .Select(x => (x.Key, x.First().Item2))
            .ToArray();

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
                var incomingDeals = x.Where(y => y.OperationId == (byte)OperationTypes.Приход).ToArray();
                var expenseDeals = x.Where(y => y.OperationId == (byte)OperationTypes.Расход).OrderByDescending(y => y.Cost).ToArray();

                var sumValue = expenseDeals.Sum(y => y.Value) - incomingDeals.Sum(y => y.Value);

                if (sumValue <= 0)
                {
                    var sumCost = expenseDeals.Sum(y => y.Cost * y.Value) - incomingDeals.Sum(y => y.Cost * y.Value);
                    return new DealSummary(x.Key, sumValue, sumCost);
                }

                var _sumCost = 0m;
                var _sumValue = 0m;

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

                    _sumCost += deal.Cost * dealValue;

                    if (sumValue == _sumValue)
                        break;
                }

                return new DealSummary(x.Key, sumValue, _sumCost);
            })
            .ToArray();
    }
    private void TransferToRecommendations(DealMqDto[] models)
    {
        var computeResult = models.Where(x => x.SumValue.HasValue).ToArray();
        var deleteResult = models.Where(x => !x.SumValue.HasValue).ToArray();

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        if (computeResult.Any())
            publisher.PublishTask(QueueNames.Recommendations, QueueEntities.Deals, QueueActions.Create, computeResult);
        if (deleteResult.Any())
            publisher.PublishTask(QueueNames.Recommendations, QueueEntities.Deals, QueueActions.Delete, deleteResult);
    }
}