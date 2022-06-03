using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Models.Api.Http;
using IM.Service.Shared.Helpers;
using IM.Service.Shared.Models.Service;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Recommendations.Services.Http;

public class PurchaseApi
{
    private readonly Repository<Purchase> purchaseRepo;
    public PurchaseApi(Repository<Purchase> purchaseRepo) => this.purchaseRepo = purchaseRepo;

    public async Task<PaginationModel<PurchaseDto>> GetAsync(ServiceHelper.Paginatior pagination)
    {
        var count = await purchaseRepo.GetCountAsync();
        var paginatedResult = purchaseRepo.GetPaginationQuery(pagination, x => x.CompanyId, x => x.Plan);

        var sales = await paginatedResult
            .Select(x => new
            {
                Company = x.Company.Name,
                x.Price,
                x.Plan,
                x.Fact
            })
            .ToArrayAsync();

        var result = sales
            .GroupBy(x => x.Company)
            .Select(x => new PurchaseDto
            {
                Company = x.Key,
                Recommendations = x
                    .OrderByDescending(y => y.Fact)
                    .ThenBy(y => y.Plan)
                    .Select(y => new PurchaseRecommendationDto
                    {
                        Plan = $"{decimal.Round(y.Plan, 1)}%",
                        Fact = y.Fact.HasValue ? $"{decimal.Round(y.Fact.Value, 1)}%" : "not compute",
                        Price = y.Price
                    })
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
                .OrderByDescending(x => x.Fact)
                .ThenBy(x => x.Plan)
                .Select(x => new PurchaseRecommendationDto
                {
                    Plan = $"{decimal.Round(x.Plan, 1)}%",
                    Fact = x.Fact.HasValue ? $"{decimal.Round(x.Fact.Value, 1)}%" : "not compute",
                    Price = x.Price
                })
                .ToArray()
        };
    }
}