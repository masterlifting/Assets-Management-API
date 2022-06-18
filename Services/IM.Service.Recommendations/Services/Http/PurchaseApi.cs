using System;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Models.Api.Http;
using IM.Service.Shared.Helpers;
using IM.Service.Shared.Models.Service;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.Http;

public class PurchaseApi
{
    private readonly Repository<Purchase> purchaseRepo;
    public PurchaseApi(Repository<Purchase> purchaseRepo) => this.purchaseRepo = purchaseRepo;

    public async Task<PaginationModel<PurchaseDto>> GetAsync(ServiceHelper.Paginatior pagination, Expression<Func<Purchase, bool>> filter)
    {
        var queryFilter = purchaseRepo.Where(filter);
        var count = await purchaseRepo.GetCountAsync(queryFilter);
        var paginatedResult = purchaseRepo.GetPaginationQueryDesc(queryFilter, pagination, x => x.Fact);

        var sales = await paginatedResult
            .OrderByDescending(x => x.Fact)
            .Select(x => new
            {
                x.CompanyId,
                Company = x.Company.Name,
                x.Price,
                x.Plan,
                x.Fact
            })
            .ToArrayAsync();

        var result = sales
            .GroupBy(x => x.CompanyId)
            .Select(x => new PurchaseDto
            {
                Company = $"({x.Key}) " + x.First().Company,
                Recommendations = x
                    .Select(y => new PurchaseRecommendationDto(y.Plan, y.Fact, y.Price))
                .ToArray()
            }).ToArray();

        return new PaginationModel<PurchaseDto>
        {
            Items = result,
            Count = count
        };
    }
    public async Task<PurchaseDto> GetAsync(string companyId)
    {
        companyId = companyId.ToUpperInvariant();
        var sales = await purchaseRepo.GetSampleAsync(x => x.CompanyId == companyId);

        return new PurchaseDto
        {
            Company = companyId,
            Recommendations = sales
                .Select(x => new PurchaseRecommendationDto(x.Plan, x.Fact, x.Price))
                .ToArray()
        };
    }
}