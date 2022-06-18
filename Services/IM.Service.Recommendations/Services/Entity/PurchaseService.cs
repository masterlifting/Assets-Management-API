using System;
using System.Collections.Generic;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Settings;
using IM.Service.Shared.Helpers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Linq;
using System.Threading.Tasks;
using IM.Service.Recommendations.Domain.DataAccess.Comparators;

namespace IM.Service.Recommendations.Services.Entity;

public class PurchaseService
{
    private const string actionsName = "Purchase recommendations";

    private readonly decimal[] percents;
    private readonly ILogger<PurchaseService> logger;
    private readonly Repository<Purchase> purchaseRepo;
    private readonly Repository<Company> companyRepo;
    public PurchaseService(
        ILogger<PurchaseService> logger,
        IOptions<ServiceSettings> options,
        Repository<Purchase> purchaseRepo,
        Repository<Company> companyRepo)
    {
        this.logger = logger;
        this.purchaseRepo = purchaseRepo;
        this.companyRepo = companyRepo;
        percents = options.Value.PurchaseSettings.DeviationPercents.OrderByDescending(x => x).ToArray();
    }
    public async Task SetAsync(Company[] companies)
    {
        var ratingCount = await companyRepo.GetCountAsync(x => x.RatingPlace.HasValue);
        var companiesWithPrices = companies.Where(x => x.PriceLast.HasValue).ToArray();
        var pricesCount = (int)companiesWithPrices.Sum(x => x.PriceLast!.Value);

        var recommendations = new List<Purchase>(pricesCount + 1);
        var processedCompanyIds = new List<string>(companiesWithPrices.Length);

        foreach (var company in companiesWithPrices)
        {
            if (!company.RatingPlace.HasValue)
            {
                logger.LogWarning(actionsName, "Rating place not found", company.Name);
                continue;
            }

            var priceLast = company.PriceLast!.Value;
            var priceAvg = company.PriceAvg!.Value;

            var companyRecommendations = GetCompanyPurchases(
                company.Id,
                priceLast,
                priceAvg,
                company.RatingPlace.Value,
                ratingCount);
            recommendations.AddRange(companyRecommendations);

            processedCompanyIds.Add(company.Id);
        }

        if (!processedCompanyIds.Any())
            return;

        var purchasesToDelete = await purchaseRepo.GetSampleAsync(x => processedCompanyIds.Contains(x.CompanyId));
        await purchaseRepo.DeleteRangeAsync(purchasesToDelete, actionsName);

        await purchaseRepo.CreateRangeAsync(recommendations, new PurchaseComparer(), actionsName);
    }
    public Task DeleteAsync(Company[] companies)
    {
        throw new NotImplementedException();
    }
    private IEnumerable<Purchase> GetCompanyPurchases(string companyId, decimal priceLast, decimal priceAvg, int ratingPlace, int ratingCount) =>
    ComputeRecommendations(priceAvg, ratingPlace, ratingCount).Select(y =>
    {
        var (plan, price) = y;
        var fact = price / priceLast * 100 - 100;
        return new Purchase
        {
            CompanyId = companyId,
            Plan = plan,
            Fact = fact,
            Price = price,
            IsReady = fact > plan
        };
    });
    private IEnumerable<(decimal Plan, decimal Price)> ComputeRecommendations(decimal priceAvg, int ratingPlace, int ratingCount)
    {
        var activeParts = ComputeActiveParts(ratingPlace, ratingCount);

        foreach (var profitPercent in percents)
        {
            if (!activeParts.ContainsKey(profitPercent))
                yield break;

            var value = priceAvg - priceAvg * activeParts[profitPercent] / 100;
            yield return (profitPercent, Math.Round(value, 5));
        }
    }
    private Dictionary<decimal, decimal> ComputeActiveParts(int ratingPlace, int ratingCount)
    {
        var ratingPercent = 100 - (decimal)ratingPlace * 100 / ratingCount;

        var result = new Dictionary<decimal, decimal>(percents.Length);

        foreach (var profitPercent in percents)
        {
            var value = profitPercent - ratingPercent * profitPercent / 100;
            result.Add(profitPercent, value);
        }

        return result;
    }
}