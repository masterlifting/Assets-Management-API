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
    private readonly Repository<Company> companyRepo;
    public SaleService(
        ILogger<SaleService> logger,
        IOptions<ServiceSettings> options,
        Repository<Sale> saleRepo,
        Repository<Company> companyRepo)
    {
        this.logger = logger;
        this.saleRepo = saleRepo;
        this.companyRepo = companyRepo;
        percents = options.Value.SaleSettings.DeviationPercents.OrderByDescending(x => x).ToArray();
    }

    public async Task SetAsync(IEnumerable<Company> companies)
    {
        var ratingCount = await companyRepo.GetCountAsync(x => x.RatingPlace.HasValue);
        var companiesWithDeals = companies.Where(x => x.DealValue.HasValue).ToArray();
        var dealsCount = Math.Abs((int)companiesWithDeals.Sum(x => x.DealValue!.Value));

        var recommendations = new List<Sale>(dealsCount + 1);
        var processedCompanyIds = new List<string>(companiesWithDeals.Length);

        foreach (var company in companiesWithDeals)
        {
            if (!company.RatingPlace.HasValue)
            {
                logger.LogWarning(actionsName, "Rating place not found", company.Name);
                continue;
            }

            var dealCost = company.DealCost!.Value;
            var dealValue = company.DealValue!.Value;

            if (dealCost == 0 && company.PriceLast.HasValue)
            {
                dealValue = Math.Abs(dealValue);
                dealCost = dealValue * company.PriceLast.Value;
            }

            var companyRecommendations = GetCompanySales(
                company.Id,
                dealValue,
                dealCost,
                company.RatingPlace.Value,
                ratingCount,
                company.PriceLast);
            recommendations.AddRange(companyRecommendations);

            processedCompanyIds.Add(company.Id);
        }

        if (!processedCompanyIds.Any())
            return;

        var salesToDelete = await saleRepo.GetSampleAsync(x => processedCompanyIds.Contains(x.CompanyId));
        await saleRepo.DeleteRangeAsync(salesToDelete, actionsName);

        await saleRepo.CreateRangeAsync(recommendations, new SaleComparer(), actionsName);
    }
    public async Task SetAsync(Company company)
    {
        if (!company.RatingPlace.HasValue)
        {
            logger.LogWarning(actionsName, "Rating place not found", company.Name);
            return;
        }
        if (!company.DealValue.HasValue || !company.DealCost.HasValue)
        {
            logger.LogWarning(actionsName, "Deal not found", company.Name);
            return;
        }

        var ratingCount = await companyRepo.GetCountAsync(x => x.RatingPlace.HasValue);

        var dealCost = company.DealCost!.Value;
        var dealValue = company.DealValue!.Value;

        if (dealCost == 0 && company.PriceLast.HasValue)
        {
            dealValue = Math.Abs(dealValue);
            dealCost = dealValue * company.PriceLast.Value;
        }

        var recommendations = GetCompanySales(
            company.Id,
            dealValue,
            dealCost,
            company.RatingPlace.Value,
            ratingCount,
            company.PriceLast)
        .ToArray();

        var salesToDelete = await saleRepo.GetSampleAsync(x => company.Id == x.CompanyId);
        await saleRepo.DeleteRangeAsync(salesToDelete, actionName);

        await saleRepo.CreateRangeAsync(recommendations, new SaleComparer(), actionName);
    }

    public async Task DeleteAsync(IEnumerable<Company> companies)
    {
        var companyIds = companies.Select(x => x.Id).Distinct();
        var sales = await saleRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId));
        await saleRepo.DeleteRangeAsync(sales, actionsName);
    }
    public async Task DeleteAsync(Company company)
    {
        var sales = await saleRepo.GetSampleAsync(x => company.Id.Equals(x.CompanyId));
        await saleRepo.DeleteRangeAsync(sales, actionsName);
    }

    private IEnumerable<Sale> GetCompanySales(string companyId, decimal sumValue, decimal sumCost, int ratingPlace, int ratingCount, decimal? lastPrice) =>
        ComputeRecommendations(sumValue, sumCost, ratingPlace, ratingCount).Select(y =>
        {
            var fact = lastPrice / y.Price * 100 - 100;
            return new Sale
            {
                CompanyId = companyId,
                Plan = y.Plan,
                Fact = fact,
                Value = y.Value,
                Price = y.Price,
                IsReady = fact is > 0
            };
        });
    private IEnumerable<(decimal Plan, decimal Value, decimal Price)> ComputeRecommendations(decimal sumValue, decimal sumCost, int ratingPlace, int ratingCount)
    {
        var _value = sumValue;
        var _cost = sumCost / sumValue;

        yield return (0, _value, Math.Round(_cost, 5));

        var activeParts = ComputeActiveParts(ratingPlace, ratingCount);

        foreach (var profitPercent in percents)
        {
            if (_value <= 0 || !activeParts.ContainsKey(profitPercent))
                yield break;

            var value = Math.Round(sumValue * activeParts[profitPercent] / 100);

            if (value == 0)
                value = _value;

            _value -= value;

            yield return (profitPercent, value, Math.Round(_cost + _cost * profitPercent / 100, 5));
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