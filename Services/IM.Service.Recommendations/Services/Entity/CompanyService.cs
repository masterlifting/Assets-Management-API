using System;
using System.Collections.Generic;
using System.Linq;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Shared.Models.RabbitMq.Api;

using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.Entity;

public sealed class CompanyService
{
    private readonly Repository<Company> companyRepo;

    public CompanyService(Repository<Company> companyRepo) => this.companyRepo = companyRepo;

    public async Task SetCompanyAsync(PriceMqDto price)
    {
        var company = await companyRepo.FindAsync(x => x.Id.Equals(price.CompanyId));
        if (company is not null)
        {
            company.PriceLast = price.PriceLast;
            company.PriceAvg = price.PriceAvg;
            await companyRepo.UpdateAsync(new object[] { company.Id }, company, $"Set company price data. CompanyId: {price.CompanyId}");
        }
    }
    public async Task SetCompanyAsync(DealMqDto deal)
    {
        var company = await companyRepo.FindAsync(x => x.Id.Equals(deal.CompanyId));
        if (company is not null)
        {
            company.DealCost = deal.SumCost;
            company.DealValue = deal.SumValue;
            await companyRepo.UpdateAsync(new object[] { company.Id }, company, $"Set company deal data. CompanyId: {deal.CompanyId}");
        }
    }
    public async Task SetCompaniesAsync(IEnumerable<PriceMqDto> prices)
    {
        var groupedData = prices
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.First());

        var dbCompanies = await companyRepo.GetSampleAsync(x => groupedData.Keys.Contains(x.Id));

        if (dbCompanies.Length != groupedData.Keys.Count)
            throw new ApplicationException("Companies from Recommendations and Market services not matched");

        var companies = dbCompanies.Join(groupedData, x => x.Id, y => y.Key, (x, y) =>
        {
            var (_, (_, priceLast, priceAvg)) = y;
            x.PriceLast = priceLast;
            x.PriceAvg = priceAvg;
            return x;
        })
        .ToArray();

        if (companies.Any())
            await companyRepo.UpdateRangeAsync(companies, $"Set company prices data. CompanyIds: {string.Join("; ", groupedData.Keys)}");
    }
    public async Task SetCompaniesAsync(IEnumerable<DealMqDto> deals)
    {
        var groupedData = deals
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.First());

        var dbCompanies = await companyRepo.GetSampleAsync(x => groupedData.Keys.Contains(x.Id));

        if (dbCompanies.Length != groupedData.Keys.Count)
            throw new ApplicationException("Companies from Recommendations and Portfolio services not matched");

        var companies = dbCompanies.Join(groupedData, x => x.Id, y => y.Key, (x, y) =>
            {
                var (_, (_, sumValue, sumCost)) = y;
                x.DealValue = sumValue;
                x.DealCost = sumCost;
                return x;
            })
            .ToArray();
        
        if (companies.Any())
            await companyRepo.UpdateRangeAsync(companies, $"Set company deals data. CompanyIds: {string.Join("; ", groupedData.Keys)}");
    }
    public async Task SetCompaniesAsync(IEnumerable<RatingMqDto> rating)
    {
        var groupedData = rating
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.First());

        var dbCompanies = await companyRepo.GetSampleAsync(x => groupedData.Keys.Contains(x.Id));

        if (dbCompanies.Length != groupedData.Keys.Count)
            throw new ApplicationException("Companies from Recommendations and Market services not matched");

        var companies = dbCompanies.Join(groupedData, x => x.Id, y => y.Key, (x, y) =>
            {
                x.RatingPlace = y.Value.Place;
                return x;
            })
            .ToArray();

        if (companies.Any())
            await companyRepo.UpdateRangeAsync(companies, $"Set company rating data. CompanyIds: {string.Join("; ", groupedData.Keys)}");
    }
}