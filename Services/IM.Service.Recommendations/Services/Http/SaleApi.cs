using System;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Models.Api.Http;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IM.Service.Shared.Models.Service;
using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Recommendations.Services.Http;

public class SaleApi
{
    private readonly Repository<Sale> saleRepo;
    public SaleApi(Repository<Sale> saleRepo) => this.saleRepo = saleRepo;

    public async Task<PaginationModel<SaleDto>> GetAsync(Paginatior pagination, Expression<Func<Sale, bool>> filter)
    {
        var queryFilter = saleRepo.Where(filter);
        var count = await saleRepo.GetCountAsync(queryFilter);
        var paginatedResult = saleRepo.GetPaginationQueryDesc(queryFilter, pagination, x => x.Fact);

        var sales = await paginatedResult
            .OrderByDescending(x => x.Fact)
            .Select(x => new
            {
                x.CompanyId,
                Company = x.Company.Name,
                x.Value,
                x.Price,
                x.Plan,
                x.Fact
            })
            .ToArrayAsync();

        var result = sales
            .GroupBy(x => x.CompanyId)
            .Select(x => new SaleDto
            {
                Company = $"({x.Key}) " + x.First().Company,
                Recommendations = x
                    .OrderBy(y => y.Plan)
                    .Select(y => new SaleRecommendationDto(y.Plan, y.Fact, y.Value, y.Price))
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
                .OrderBy(x => x.Plan)
                .Select(x => new SaleRecommendationDto(x.Plan, x.Fact, x.Value, x.Price))
                .ToArray()
        };
    }
}