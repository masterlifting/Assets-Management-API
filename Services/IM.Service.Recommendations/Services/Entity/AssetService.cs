using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Shared.Models.RabbitMq.Api;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.Entity;

public sealed class AssetService
{
    private const string logPrefix = $"{nameof(AssetService)}";
    private const string actionName = "Set asset";
    private const string actionsName = "Set assets";

    private readonly Repository<Asset> assetRepo;
    public AssetService(Repository<Asset> assetRepo) => this.assetRepo = assetRepo;

    public async Task<Asset> SetAsync(CostMqDto cost)
    {
        var (assetId, assetTypeId, priceLast, priceAvg) = cost;
        var asset = await assetRepo.FindAsync(assetId, assetTypeId );
        if (asset is not null)
        {
            asset.CostFact = priceLast;
            asset.CostAvg = priceAvg;

            await assetRepo.UpdateAsync(new object[] { asset.Id, assetTypeId }, asset, actionName + ':' + assetId);
            return asset;
        }
        throw new DataException($"{logPrefix}.{actionName}.Error: {assetId} not found");
    }
    public async Task<Asset> SetAsync(DealMqDto deal)
    {
        var (assetId, assetTypeId, sumValue, sumCost, dealPrice) = deal;
        var asset = await assetRepo.FindAsync( assetId, assetTypeId );
        if (asset is not null)
        {
            asset.DealSumCost = sumCost;
            asset.DealSumValue = sumValue;
            asset.DealCostLast = dealPrice;
            await assetRepo.UpdateAsync(new object[] { asset.Id, assetTypeId }, asset, actionName + ':' + assetId);
            return asset;
        }
        throw new DataException($"{logPrefix}.{actionName}.Error: {assetId} not found");
    }
    public async Task<Asset[]> SetAsync(IEnumerable<CostMqDto> costs)
    {
        var groupedData = costs
            .GroupBy(x => (x.AssetId, x.AssetTypeId))
            .ToDictionary(x => x.Key, y => y.First());

        var assetIds = groupedData.Select(x => x.Key.AssetId).Distinct();
        var assetTypeIds = groupedData.Select(x => x.Key.AssetTypeId).Distinct();
        var dbAssets = await assetRepo.GetSampleAsync(x => assetIds.Contains(x.Id) && assetTypeIds.Contains(x.TypeId));

        var assets = dbAssets.Join(groupedData, x => (x.Id, x.TypeId), y => y.Key, (x, y) =>
        {
            var (_, (_, _, costFact, costAvg)) = y;
            x.CostFact = costFact;
            x.CostAvg = costAvg;
            return x;
        })
        .ToArray();

        await assetRepo.UpdateRangeAsync(assets, actionsName);
        return assets;
    }
    public async Task<Asset[]> SetAsync(IEnumerable<DealMqDto> deals)
    {
        var groupedData = deals
            .GroupBy(x => (x.AssetId, x.AssetTypeId))
            .ToDictionary(x => x.Key, y => y.First());

        var assetIds = groupedData.Select(x => x.Key.AssetId).Distinct();
        var assetTypeIds = groupedData.Select(x => x.Key.AssetTypeId).Distinct();
        var dbAssets = await assetRepo.GetSampleAsync(x => assetIds.Contains(x.Id) && assetTypeIds.Contains(x.TypeId));

        var assets = dbAssets.Join(groupedData, x => (x.Id, x.TypeId), y => y.Key, (x, y) =>
            {
                var (_, (_, _, sumValue, sumCost, costLast)) = y;
                x.DealSumValue = sumValue;
                x.DealSumCost = sumCost;
                x.DealCostLast = costLast;
                return x;
            })
            .ToArray();

        await assetRepo.UpdateRangeAsync(assets, actionsName);
        return assets;
    }
    public async Task<Asset[]> SetAsync(IEnumerable<RatingMqDto> rating)
    {
        var groupedData = rating
            .GroupBy(x => (x.AssetId, x.AssetTypeId))
            .ToDictionary(x => x.Key, y => y.First());

        var assetIds = groupedData.Select(x => x.Key.AssetId).Distinct();
        var assetTypeIds = groupedData.Select(x => x.Key.AssetTypeId).Distinct();
        var dbAssets = await assetRepo.GetSampleAsync(x => assetIds.Contains(x.Id) && assetTypeIds.Contains(x.TypeId));

        var assets = dbAssets.Join(groupedData, x => (x.Id, x.TypeId), y => y.Key, (x, y) =>
        {
            x.RatingPlace = y.Value.Place;
            return x;
        })
        .ToArray();

        await assetRepo.UpdateRangeAsync(assets, actionsName);
        return assets;
    }
}