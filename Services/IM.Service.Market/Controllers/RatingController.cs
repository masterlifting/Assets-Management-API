using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi;
using Microsoft.AspNetCore.Mvc;
using static IM.Service.Common.Net.Enums;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class RatingController : ControllerBase
{
    private readonly RatingRestApi api;
    public RatingController(RatingRestApi api) => this.api = api;

    [HttpGet]
    public Task<ResponseModel<PaginatedModel<RatingGetDto>>> Get(int page = 0, int limit = 0) => 
        api.GetAsync(new HttpPagination(page, limit), nameof(Rating));

    [HttpGet("{companyId}")]
    public Task<ResponseModel<RatingGetDto>> GetByCompany(string companyId) => api.GetAsync(companyId);
    
    [HttpGet("{place:int}")]
    public Task<ResponseModel<RatingGetDto>> GetByPlace(int place) => api.GetAsync(place);
    
    [HttpGet("price/")]
    public Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetPriceResultOrdered(int page = 0, int limit = 0) =>
        api.GetAsync(new HttpPagination(page, limit), nameof(Price));

    [HttpGet("report/")]
    public Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetReportResultOrdered(int page = 0, int limit = 0) =>
        api.GetAsync(new HttpPagination(page, limit), nameof(Report));

    [HttpGet("coefficient/")]
    public Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetCoefficientResultOrdered(int page = 0, int limit = 0) =>
        api.GetAsync(new HttpPagination(page, limit), nameof(Coefficient));

    [HttpGet("dividend/")]
    public Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetDividendResultOrdered(int page = 0, int limit = 0) =>
        api.GetAsync(new HttpPagination(page, limit), nameof(Dividend));


    [HttpGet("recalculate/")]
    public Task<string> Recalculate() => api.RecalculateAsync();
    [HttpGet("recalculate/{companyId}")]
    public Task<string> Recalculate(string companyId)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1 ? api.RecalculateAsync(companyIds) : api.RecalculateAsync(companyId);
    }
    [HttpGet("recalculate/{companyId}/{year:int}")]
    public Task<string> Recalculate(string companyId, int year)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, companyIds, year))
            : api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, companyId, year));
    }
    [HttpGet("recalculate/{companyId}/{year:int}/{month:int}")]
    public Task<string> Recalculate(string companyId, int year, int month)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, companyIds, year, month))
            : api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, companyId, year, month));
    }
    [HttpGet("recalculate/{companyId}/{year:int}/{month:int}/{day:int}")]
    public Task<string> Recalculate(string companyId, int year, int month, int day)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, companyIds, year, month, day))
            : api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, companyId, year, month, day));
    }
    [HttpGet("recalculate/{year:int}")]
    public Task<string> Recalculate(int year) => api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, year));
    [HttpGet("recalculate/{year:int}/{month:int}")]
    public Task<string> Recalculate(int year, int month) => api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, year, month));
    [HttpGet("recalculate/{year:int}/{month:int}/{day:int}")]
    public Task<string> Recalculate(int year, int month, int day) => api.RecalculateAsync(new CompanyDateFilter<Rating>(CompareType.Equal, year, month, day));
}