using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.DataAccess.Comparators;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Settings;
using IM.Service.Shared.Helpers;
using IM.Service.Shared.Models.RabbitMq.Api;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.Entity;

public sealed class SaleService
{
    private readonly decimal[] profitPercents;
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
        profitPercents = options.Value.SaleSettings.DeviationPercents.OrderByDescending(x => x).ToArray();
    }

    public async Task SetAsync(IEnumerable<DealMqDto> entities)
    {
        var groupedDeals = entities
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.First());

        var companies = await companyRepo.GetSampleAsync(x => groupedDeals.Keys.Contains(x.Id));

        var ratingCount = await companyRepo.GetCountAsync(x => x.RatingPlace.HasValue);
        var dealsCount = (int)companies.Sum(x => x.DealValue!.Value);

        var recommendations = new List<Sale>(dealsCount + 1);
        var processedCompanyIds = new List<string>(companies.Length);

        foreach (var company in companies)
        {
            if (!company.RatingPlace.HasValue)
            {
                logger.LogWarning(nameof(SetAsync), "Rating place not found", company.Name);
                continue;
            }

            var companyRecommendations = GetCompanySales(company.Id, company.DealValue!.Value, company.DealCost!.Value, company.RatingPlace.Value, ratingCount);
            recommendations.AddRange(companyRecommendations);
            processedCompanyIds.Add(company.Id);
        }

        if (!processedCompanyIds.Any())
        {
            logger.LogWarning(nameof(SetAsync), "Sale recommendations", "not computed");
            return;
        }

        var salesToDelete = await saleRepo.GetSampleAsync(x => processedCompanyIds.Contains(x.CompanyId));

        await saleRepo.DeleteRangeAsync(salesToDelete, nameof(SetAsync) + $". CompanyIds: {string.Join("; ", salesToDelete.Select(x => x.CompanyId).Distinct())}");
        await saleRepo.CreateRangeAsync(recommendations, new SaleComparer(), nameof(SetAsync) + $". CompanyIds: {string.Join("; ", processedCompanyIds)}");
    }
    public async Task SetAsync(IEnumerable<RatingMqDto> entities)
    {
        var groupedRatings = entities
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.First());

        var companies = await companyRepo.GetSampleAsync(x => groupedRatings.Keys.Contains(x.Id));

        var dealsCount = (int)companies.Where(x => x.DealValue.HasValue).Sum(x => x.DealValue!.Value);

        var recommendations = new List<Sale>(dealsCount + 1);
        var processedCompanyIds = new List<string>(companies.Length);

        foreach (var company in companies)
        {
            if (!company.DealValue.HasValue || !company.DealCost.HasValue)
            {
                logger.LogWarning(nameof(SetAsync), "Deal data not found", company.Name);
                continue;
            }

            var companyRecommendations = GetCompanySales(company.Id, company.DealValue!.Value, company.DealCost!.Value, company.RatingPlace!.Value, groupedRatings.Count);
            recommendations.AddRange(companyRecommendations);
            processedCompanyIds.Add(company.Id);
        }

        if (!processedCompanyIds.Any())
        {
            logger.LogWarning(nameof(SetAsync), "Sele recommendations", "not computed");
            return;
        }

        var salesToDelete = await saleRepo.GetSampleAsync(x => processedCompanyIds.Contains(x.CompanyId));

        await saleRepo.DeleteRangeAsync(salesToDelete, nameof(SetAsync) + $". CompanyIds: {string.Join("; ", salesToDelete.Select(x => x.CompanyId).Distinct())}");
        await saleRepo.CreateRangeAsync(recommendations, new SaleComparer(), nameof(SetAsync) + $". CompanyIds: {string.Join("; ", recommendations.Select(x => x.CompanyId))}");
    }

    public async Task DeleteAsync(DealMqDto[] entities)
    {
        var sales = await saleRepo.GetSampleAsync(x => entities.Select(y => y.CompanyId).Contains(x.CompanyId));
        await saleRepo.DeleteRangeAsync(sales, nameof(DeleteAsync));
    }
    public async Task DeleteAsync(RatingMqDto[] entities)
    {
        var sales = await saleRepo.GetSampleAsync(x => entities.Select(y => y.CompanyId).Contains(x.CompanyId));
        await saleRepo.DeleteRangeAsync(sales, nameof(DeleteAsync));
    }


    private IEnumerable<Sale> GetCompanySales(string companyId, decimal sumValue, decimal sumCost, int ratingPlace, int ratingCount) =>
        ComputeRecommendations(sumValue, sumCost, ratingPlace, ratingCount).Select(y => new Sale
        {
            CompanyId = companyId,
            Plan = y.ProfitPercent,
            Value = y.Value,
            Price = y.Price
        });
    private IEnumerable<(decimal ProfitPercent, decimal Value, decimal Price)> ComputeRecommendations(decimal sumValue, decimal sumCost, int ratingPlace, int ratingCount)
    {
        var _value = sumValue;
        var _cost = sumCost / sumValue;
        var activeParts = ComputeActiveParts(ratingPlace, ratingCount);

        foreach (var profitPercent in profitPercents)
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
        var partsCount = profitPercents.Length;
        var ratingPercent = 100 - (decimal)ratingPlace * 100 / ratingCount;
        decimal resultPercent = 100;

        var result = new Dictionary<decimal, decimal>(profitPercents.Length);
        foreach (var profitPercent in profitPercents)
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

    internal Task SetAsync(DealMqDto entities)
    {
        throw new NotImplementedException();
    }
    internal Task DeleteAsync(DealMqDto entities)
    {
        throw new NotImplementedException();
    }
}