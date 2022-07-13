using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.DataAccess.Comparators;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Settings;
using IM.Service.Recommendations.Settings.Sections;
using IM.Service.Shared.Helpers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.Entity;

public class PurchaseService
{
    private const string actionsName = "Purchase recommendations";

    private readonly PurchaseSettings settings;
    private readonly ILogger<PurchaseService> logger;
    private readonly Repository<Purchase> purchaseRepo;
    private readonly Repository<Asset> assetRepo;
    public PurchaseService(
        ILogger<PurchaseService> logger,
        IOptionsSnapshot<ServiceSettings> options,
        Repository<Purchase> purchaseRepo,
        Repository<Asset> assetRepo)
    {
        this.logger = logger;
        this.purchaseRepo = purchaseRepo;
        this.assetRepo = assetRepo;
        settings = options.Value.PurchaseSettings;
    }

    public async Task SetAsync(Asset[] assets)
    {
        var ratingCount = await assetRepo.GetCountAsync(x => x.RatingPlace.HasValue);
        var assetsWithPrices = assets.Where(x => x.CostFact.HasValue).ToArray();
        var costCount = (int)assetsWithPrices.Sum(x => x.CostFact!.Value);

        var purchases = new List<Purchase>(costCount + 1);
        var processedIds = new List<(string AssetId, byte AssetTypeId)>(assetsWithPrices.Length);

        foreach (var asset in assetsWithPrices)
        {
            int ratingPlace;

            if (asset.RatingPlace.HasValue)
                ratingPlace = asset.RatingPlace.Value;
            else
            {
                logger.LogWarning(actionsName, "Rating place not found", asset.Name);
                ratingPlace = 1;
                ratingCount = 1;
            }

            var costFact = asset.CostFact!.Value;

            if (costFact == 0)
            {
                logger.LogWarning(actionsName, "Cost fact = 0", asset.Name);
                continue;
            }

            var costAvg = asset.CostAvg!.Value;

            purchases.Add(GetPurchase(
                asset.Id,
                asset.TypeId,
                costFact,
                costAvg,
                ratingPlace,
                ratingCount,
                asset.DealCostLast));

            processedIds.Add((asset.Id, asset.TypeId));
        }

        if (!processedIds.Any())
            return;

        var assetIds = processedIds.Select(x => x.AssetId).Distinct();
        var assetTypeIds = processedIds.Select(x => x.AssetTypeId).Distinct();

        var purchasesToDelete = await purchaseRepo.GetSampleAsync(x => assetIds.Contains(x.AssetId) && assetTypeIds.Contains(x.AssetTypeId));
        await purchaseRepo.DeleteRangeAsync(purchasesToDelete, actionsName);

        await purchaseRepo.CreateRangeAsync(purchases, new PurchaseComparer(), actionsName);
    }
    public Task DeleteAsync(Asset[] assets)
    {
        throw new NotImplementedException();
    }

    private Purchase GetPurchase(string assetId, byte assetTypeId, decimal costFact, decimal costAvg, int ratingPlace, int ratingCount, decimal? dealCost)
    {
        var (discountPlan, pricePlan) = ComputeRecommendation(costAvg, ratingPlace, ratingCount);
        var discountFact = pricePlan / costFact * 100 - 100;

        decimal? costNext = null;
        decimal? percentStep = null;
        if (dealCost.HasValue && dealCost.Value != 0)
        {
            percentStep = costFact * 100 / dealCost.Value - 100;
            var discountStep = settings.DiscountStep * (percentStep.Value / Math.Abs(percentStep.Value));
            costNext = dealCost.Value + discountStep / 100 * dealCost.Value;
        }

        return new Purchase
        {
            AssetId = assetId,
            AssetTypeId = assetTypeId,
            DiscountPlan = discountPlan,
            DiscountFact = discountFact,
            CostFact = costFact,
            CostPlan = pricePlan,
            CostNext = costNext,
            IsReady = discountFact >= discountPlan || percentStep.HasValue && Math.Abs(percentStep.Value) >= settings.DiscountStep
        };
    }
    private (decimal DiscountPlan, decimal CostPlan) ComputeRecommendation(decimal costAvg, int ratingPlace, int ratingCount)
    {
        var discountPercent = ComputeDiscountPercent(ratingPlace, ratingCount);
        var costPlan = costAvg - costAvg * discountPercent / 100;

        return (discountPercent, Math.Round(costPlan, 10));
    }
    private decimal ComputeDiscountPercent(int ratingPlace, int ratingCount)
    {
        var ratingPercent = 100 - (decimal)(ratingPlace - 1) / ratingCount * 100;
        return settings.DiscountMax - (settings.DiscountMax - settings.DiscountMin) * ratingPercent / 100;
    }
}