using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Shared.Models.RabbitMq.Api;

using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.Entity;

public sealed class CompanyService
{
    private const string logPrefix = $"{nameof(CompanyService)}";
    private const string actionName = "Set company";
    private const string actionsName = "Set companies";

    private readonly Repository<Company> companyRepo;

    public CompanyService(Repository<Company> companyRepo) => this.companyRepo = companyRepo;

    public async Task<Company> SetCompanyAsync(PriceMqDto price)
    {
        var (companyId, priceLast, priceAvg) = price;
        var company = await companyRepo.FindAsync(x => x.Id.Equals(companyId));
        if (company is not null)
        {
            company.PriceLast = priceLast;
            company.PriceAvg = priceAvg;
            await companyRepo.UpdateAsync(new object[] { company.Id }, company, actionName + ':' +companyId);
            return company;
        }
        throw new DataException($"{logPrefix}.{actionName}.Error: {companyId} not found");
    }
    public async Task<Company> SetCompanyAsync(DealMqDto deal)
    {
        var (companyId, sumValue, sumCost) = deal;
        var company = await companyRepo.FindAsync(x => x.Id.Equals(companyId));
        if (company is not null)
        {
            company.DealCost = sumCost;
            company.DealValue = sumValue;
            await companyRepo.UpdateAsync(new object[] { company.Id }, company, actionName + ':' + companyId);
            return company;
        }
        throw new DataException($"{logPrefix}.{actionName}.Error: {companyId} not found");
    }
    public async Task<Company[]> SetCompaniesAsync(IEnumerable<PriceMqDto> prices)
    {
        var groupedData = prices
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.First());

        var dbCompanies = await companyRepo.GetSampleAsync(x => groupedData.Keys.Contains(x.Id));

        if (dbCompanies.Length != groupedData.Keys.Count)
            throw new ApplicationException($"{logPrefix}.{actionsName}.Error: Companies from Recommendations and Market services not matched");

        var companies = dbCompanies.Join(groupedData, x => x.Id, y => y.Key, (x, y) =>
        {
            var (_, (_, priceLast, priceAvg)) = y;
            x.PriceLast = priceLast;
            x.PriceAvg = priceAvg;
            return x;
        })
        .ToArray();

        await companyRepo.UpdateRangeAsync(companies, actionsName);
        return companies;
    }
    public async Task<Company[]> SetCompaniesAsync(IEnumerable<DealMqDto> deals)
    {
        var groupedData = deals
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.First());

        var dbCompanies = await companyRepo.GetSampleAsync(x => groupedData.Keys.Contains(x.Id));

        if (dbCompanies.Length != groupedData.Keys.Count)
            throw new ApplicationException($"{logPrefix}.{actionsName}.Error: Companies from Recommendations and Portfolio services not matched");

        var companies = dbCompanies.Join(groupedData, x => x.Id, y => y.Key, (x, y) =>
            {
                var (_, (_, sumValue, sumCost)) = y;
                x.DealValue = sumValue;
                x.DealCost = sumCost;
                return x;
            })
            .ToArray();

        await companyRepo.UpdateRangeAsync(companies, actionsName);
        return companies;
    }
    public async Task<Company[]> SetCompaniesAsync(IEnumerable<RatingMqDto> rating)
    {
        var groupedData = rating
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.First());

        var dbCompanies = await companyRepo.GetSampleAsync(x => groupedData.Keys.Contains(x.Id));

        if (dbCompanies.Length != groupedData.Keys.Count)
            throw new ApplicationException($"{logPrefix}.{actionsName}.Error: Companies from Recommendations and Market services not matched");

        var companies = dbCompanies.Join(groupedData, x => x.Id, y => y.Key, (x, y) =>
            {
                x.RatingPlace = y.Value.Place;
                return x;
            })
            .ToArray();

        await companyRepo.UpdateRangeAsync(companies, actionsName);
        return companies;
    }
}