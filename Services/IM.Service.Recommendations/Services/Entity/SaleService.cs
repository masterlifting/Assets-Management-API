using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.DataAccess.Comparators;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Settings;
using IM.Service.Shared.Helpers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.Entity;

public sealed class SaleService
{
    private const string actionsName = "Sale recommendations";
    private const string actionName = "Sale recommendation";

    private readonly decimal[] percents;
    private readonly ILogger<SaleService> logger;
    private readonly Repository<Sale> saleRepo;
    private readonly Repository<Asset> assetRepo;
    public SaleService(
        ILogger<SaleService> logger,
        IOptionsSnapshot<ServiceSettings> options,
        Repository<Sale> saleRepo,
        Repository<Asset> assetRepo)
    {
        this.logger = logger;
        this.saleRepo = saleRepo;
        this.assetRepo = assetRepo;
        percents = options.Value.SaleSettings.Profits.OrderByDescending(x => x).ToArray();
    }

    public async Task SetAsync(IEnumerable<Asset> assets)
    {
        var ratingCount = await assetRepo.GetCountAsync(x => x.RatingPlace.HasValue);
        var assetsWithDeals = assets.Where(x => x.DealSumValue.HasValue).ToArray();
        var dealsCount = Math.Abs((int)assetsWithDeals.Sum(x => x.DealSumValue!.Value));

        var recommendations = new List<Sale>(dealsCount + 1);
        var processedIds = new List<(string AssetId, byte AssetTypeId)>(assetsWithDeals.Length);

        foreach (var asset in assetsWithDeals)
        {
            int ratingPlace;

            if (asset.RatingPlace.HasValue)
                ratingPlace = asset.RatingPlace.Value;
            else
            {
                logger.LogWarning(actionsName, "Rating place not found", asset.Name);
                ratingPlace = percents.Length;
                ratingCount = percents.Length;
            }

            var dealCost = asset.DealSumCost!.Value;
            var dealValue = asset.DealSumValue!.Value;

            if (dealCost == 0 && asset.CostFact.HasValue)
            {
                dealValue = Math.Abs(dealValue);
                dealCost = dealValue * asset.CostFact.Value;
            }

            recommendations.AddRange(GetSales(
                asset.Id,
                asset.TypeId,
                dealValue,
                dealCost,
                ratingPlace,
                ratingCount,
                asset.CostFact));

            processedIds.Add((asset.Id, asset.TypeId));
        }

        if (!processedIds.Any())
            return;

        var assetIds = processedIds.Select(x => x.AssetId).Distinct();
        var assetTypeIds = processedIds.Select(x => x.AssetTypeId).Distinct();

        var salesToDelete = await saleRepo.GetSampleAsync(x => assetIds.Contains(x.AssetId) && assetTypeIds.Contains(x.AssetTypeId));
        await saleRepo.DeleteRangeAsync(salesToDelete, actionsName);

        await saleRepo.CreateRangeAsync(recommendations, new SaleComparer(), actionsName);
    }
    public async Task SetAsync(Asset asset)
    {
        if (!asset.DealSumValue.HasValue || !asset.DealSumCost.HasValue)
        {
            logger.LogWarning(actionsName, "Deal not found", asset.Name);
            return;
        }

        int ratingCount;
        int ratingPlace;
        if (asset.RatingPlace.HasValue)
        {
            ratingCount = await assetRepo.GetCountAsync(x => x.RatingPlace.HasValue);
            ratingPlace = asset.RatingPlace.Value;
        }
        else
        {
            logger.LogWarning(actionsName, "Rating place not found", asset.Name);
            ratingCount = percents.Length;
            ratingPlace = percents.Length;
        }

        var dealCost = asset.DealSumCost!.Value;
        var dealValue = asset.DealSumValue!.Value;

        if (dealCost == 0 && asset.CostFact.HasValue)
        {
            dealValue = Math.Abs(dealValue);
            dealCost = dealValue * asset.CostFact.Value;
        }

        var recommendations = GetSales(
            asset.Id,
            asset.TypeId,
            dealValue,
            dealCost,
            ratingPlace,
            ratingCount,
            asset.CostFact)
        .ToArray();

        var salesToDelete = await saleRepo.GetSampleAsync(x => asset.Id == x.AssetId && asset.TypeId == x.AssetTypeId);
        await saleRepo.DeleteRangeAsync(salesToDelete, actionName);

        await saleRepo.CreateRangeAsync(recommendations, new SaleComparer(), actionName);
    }

    public async Task DeleteAsync(IEnumerable<Asset> assets)
    {
        var _assets = assets.ToArray();

        var assetIds = _assets.Select(x => x.Id).Distinct();
        var assetTypeIds = _assets.Select(x => x.TypeId).Distinct();

        var sales = await saleRepo.GetSampleAsync(x => assetIds.Contains(x.AssetId) && assetTypeIds.Contains(x.AssetTypeId));

        await saleRepo.DeleteRangeAsync(sales, actionsName);
    }
    public async Task DeleteAsync(Asset asset)
    {
        var sales = await saleRepo.GetSampleAsync(x => asset.Id == x.AssetId && asset.TypeId == x.AssetTypeId);
        await saleRepo.DeleteRangeAsync(sales, actionsName);
    }

    private IEnumerable<Sale> GetSales(string assetId, byte assetTypeId, decimal sumValue, decimal sumCost, int ratingPlace, int ratingCount, decimal? costFact) =>
        ComputeRecommendations(sumValue, sumCost, ratingPlace, ratingCount).Select(y =>
        {
            var profitFact = costFact / y.CostPlan * 100 - 100;
            return new Sale
            {
                AssetId = assetId,
                AssetTypeId = assetTypeId,
                ProfitPlan = y.ProfitPlan,
                ProfitFact = profitFact,
                CostPlan = y.CostPlan,
                CostFact = costFact,
                ActiveValue = y.ActiveValue,
                IsReady = profitFact is > 0
            };
        });
    private IEnumerable<(decimal ProfitPlan, decimal ActiveValue, decimal CostPlan)> ComputeRecommendations(decimal sumValue, decimal sumCost, int ratingPlace, int ratingCount)
    {
        var _value = sumValue;
        var _cost = sumCost / sumValue;

        yield return (0, _value, Math.Round(_cost, 10));

        var activeParts = ComputeActiveParts(ratingPlace, ratingCount);

        foreach (var profitPercent in percents)
        {
            if (_value <= 0 || !activeParts.ContainsKey(profitPercent))
                yield break;

            var value = Math.Round(sumValue * activeParts[profitPercent] / 100);

            if (value == 0)
                value = _value;

            _value -= value;

            var costPlan = _cost + _cost * profitPercent / 100;
            yield return (profitPercent, value, Math.Round(costPlan, 10));
        }
    }
    private Dictionary<decimal, decimal> ComputeActiveParts(int ratingPlace, int ratingCount)
    {
        var partsCount = percents.Length;
        var ratingPercent = 100 - (decimal)ratingPlace * 100 / ratingCount;
        decimal resultPercent = 100;

        var result = new Dictionary<decimal, decimal>(percents.Length);
        foreach (var profitPercent in percents)
        {
            if (resultPercent <= 0)
                break;

            if (partsCount == 1)
            {
                result.Add(profitPercent, resultPercent);
                break;
            }

            var _resultPercent = resultPercent * ratingPercent / 100;

            result.Add(profitPercent, _resultPercent);

            resultPercent -= _resultPercent;
            partsCount--;
        }

        return result;
    }
}