using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IM.Service.Common.Net;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Company.Analyzer.DataAccess.Entities;

namespace IM.Service.Company.Analyzer.Controllers;

[ApiController, Route("[controller]")]
public class RatingsController : ControllerBase
{
    private readonly RatingDtoManager manager;
    public RatingsController(RatingDtoManager manager) => this.manager = manager;

    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> Get(int page = 0, int limit = 0) =>
        await manager.GetAsync(new HttpPagination(page, limit));

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<RatingGetDto>> Get(string companyId) => await manager.GetAsync(companyId);

    [HttpGet("{place:int}")]
    public async Task<ResponseModel<RatingGetDto>> Get(int place) => await manager.GetAsync(place);

    [HttpGet("price/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetPriceResultOrdered(int page = 0, int limit = 0) =>
        await manager.GetPriceResultOrderedAsync(new HttpPagination(page, limit));
    [HttpGet("report/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetReportResultOrdered(int page = 0, int limit = 0) =>
        await manager.GetReportResultOrderedAsync(new HttpPagination(page, limit));
    [HttpGet("coefficient/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetCoefficientResultOrdered(int page = 0, int limit = 0) =>
        await manager.GetCoefficientResultOrderedAsync(new HttpPagination(page, limit));


    [HttpGet("recalculate/")]
    public async Task<string> Recalculate() => await manager.RecalculateAsync();
    
    [HttpGet("recalculate/{companyId}")]
    public async Task<string> Recalculate(string companyId)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1 ? await manager.RecalculateAsync(companyIds) : await manager.RecalculateAsync(companyId);
    }

    [HttpGet("recalculate/{companyId}/{year:int}")]
    public async Task<string> Recalculate(string companyId, int year)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(companyIds, year))
            : await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(companyId, year));
    }

    [HttpGet("recalculate/{companyId}/{year:int}/{month:int}")]
    public async Task<string> Recalculate(string companyId, int year, int month)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(companyIds, year, month))
            : await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(companyId, year, month));
    }

    [HttpGet("recalculate/{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<string> Recalculate(string companyId, int year, int month, int day)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(CommonEnums.HttpRequestFilterType.Equal, companyIds, year, month,day))
            : await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(CommonEnums.HttpRequestFilterType.Equal, companyId, year, month,day));
    }

    [HttpGet("recalculate/{year:int}")]
    public async Task<string> Recalculate(int year) =>
        await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(year));

    [HttpGet("recalculate/{year:int}/{month:int}")]
    public async Task<string> Recalculate(int year, int month) =>
        await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(year, month));

    [HttpGet("recalculate/{year:int}/{month:int}/{day:int}")]
    public async Task<string> Recalculate(int year, int month, int day) =>
        await manager.RecalculateAsync(new CompanyDataFilterByDate<Rating>(CommonEnums.HttpRequestFilterType.Equal, year, month, day));
}