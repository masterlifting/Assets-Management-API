using IM.Service.Shared.Helpers;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Models.Api.Http;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;
using IM.Service.Shared.Models.Service;

namespace IM.Service.Recommendations.Services.Http;

public class SaleApi
{
    private readonly Repository<Sale> saleRepo;
    public SaleApi(Repository<Sale> saleRepo) => this.saleRepo = saleRepo;

    public async Task<PaginationModel<SaleDto>> GetAsync(ServiceHelper.Paginatior pagination)
    {
        var count = await saleRepo.GetCountAsync();
        var paginatedResult = saleRepo.GetPaginationQuery(pagination, x => x.CompanyId, x => x.Plan);

        var sales = await paginatedResult
            .Select(x => new
            {
                Company = x.Company.Name,
                x.Value,
                x.Price,
                x.Plan,
                x.Fact
            })
            .ToArrayAsync();

        var result = sales
            .GroupBy(x => x.Company)
            .Select(x => new SaleDto
            {
                Company = x.Key,
                Recommendations = x
                    .OrderByDescending(y => y.Fact)
                    .ThenBy(y => y.Plan)
                    .Select(y => new SaleRecommendationDto
                    {
                        Plan = $"{decimal.Round(y.Plan, 1)}%",
                        Fact =y.Fact.HasValue ? $"{decimal.Round(y.Fact.Value, 1)}%": "not compute",
                        Value = y.Value,
                        Price = y.Price
                    })
                .ToArray()
            }).ToArray();

        return new PaginationModel<SaleDto>
        {
            Items = result,
            Count = count
        };
    }
    public async Task<SaleDto> GetAsync(string companyId)
    {
        companyId = companyId.ToUpperInvariant();
        var sales = await saleRepo.GetSampleAsync(x => x.CompanyId == companyId);

        return new SaleDto
        {
            Company = companyId,
            Recommendations = sales
                .OrderByDescending(x => x.Fact)
                .ThenBy(x => x.Plan)
                .Select(x => new SaleRecommendationDto
                {
                    Plan = $"{decimal.Round(x.Plan, 1)}%",
                    Fact = x.Fact.HasValue ? $"{decimal.Round(x.Fact.Value, 1)}%" : "not compute",
                    Value = x.Value,
                    Price = x.Price
                })
                .ToArray()
        };
    }
}