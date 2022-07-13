using System;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Models.Api.Http;
using IM.Service.Shared.Models.Service;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IM.Service.Recommendations.Domain.Entities.Catalogs;
using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Recommendations.Services.Http;

public class PurchaseApi
{
    private readonly Repository<Asset> assetRepo;
    private readonly Repository<AssetType> assetTypeRepo;
    private readonly Repository<Purchase> purchaseRepo;
    public PurchaseApi(Repository<Asset> assetRepo, Repository<AssetType> assetTypeRepo, Repository<Purchase> purchaseRepo)
    {
        this.assetRepo = assetRepo;
        this.assetTypeRepo = assetTypeRepo;
        this.purchaseRepo = purchaseRepo;
    }

    public async Task<PaginationModel<PurchaseDto>> GetAsync(Paginatior pagination, Expression<Func<Purchase, bool>> filter)
    {
        var queryFilter = purchaseRepo.Where(filter);
        var count = await purchaseRepo.GetCountAsync(queryFilter);
        var paginatedResult = purchaseRepo.GetPaginationQueryDesc(queryFilter, pagination, x => x.DiscountFact);

        var purchases = await paginatedResult
            .OrderByDescending(x => x.DiscountFact)
            .Select(x => new
            {
                x.AssetId,
                x.AssetTypeId,
                x.Asset.Name,
                x.DiscountPlan,
                x.DiscountFact,
                x.CostFact,
                x.CostPlan,
                x.CostNext
            })
            .ToArrayAsync();

        var result = purchases
            .GroupBy(x => (x.AssetId, x.AssetTypeId))
            .Select(x => new PurchaseDto
            {
                Asset = $"({x.Key.AssetId}) " + x.First().Name,
                Recommendations = x
                    .Select(y => new PurchaseRecommendationDto(y.DiscountPlan, y.DiscountFact, y.CostPlan, y.CostFact, y.CostNext))
                .ToArray()
            }).ToArray();

        return new PaginationModel<PurchaseDto>
        {
            Items = result,
            Count = count
        };
    }
    public async Task<PurchaseDto> GetAsync(byte assetTypeId, string assetId)
    {
        assetId = assetId.ToUpperInvariant();

        var asset = await assetRepo.FindAsync(assetId, assetTypeId);

        if (asset is null)
            throw new NullReferenceException($"'{assetId}' not found");

        var sales = await purchaseRepo.GetSampleAsync(x => x.AssetId == asset.Id && x.AssetTypeId == asset.TypeId);

        return new PurchaseDto
        {
            Asset = $"({asset.Id}) " + asset.Name,
            Recommendations = sales
                .Select(x => new PurchaseRecommendationDto(x.DiscountPlan, x.DiscountFact, x.CostPlan, x.CostFact, x.CostNext))
                .ToArray()
        };
    }
    public async Task<AssetTypeDto[]> GetAssetsAsync()
    {
        var entities = await assetTypeRepo.GetSampleAsync(x => x);
        return entities.Select(x => new AssetTypeDto(x.Id, x.Name, x.Description)).ToArray();
    }
}