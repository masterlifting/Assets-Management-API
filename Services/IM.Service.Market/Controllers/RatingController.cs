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

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<RatingGetDto>> GetByCompany(string companyId) => await api.GetAsync(companyId);
    [HttpGet("{place:int}")]
    public async Task<ResponseModel<RatingGetDto>> GetByPlace(int place) => await api.GetAsync(place);
    [HttpGet("price/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetPriceResultOrdered(int page = 0, int limit = 0) =>
        await api.GetPriceResultOrderedAsync(new HttpPagination(page, limit));
    [HttpGet("report/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetReportResultOrdered(int page = 0, int limit = 0) =>
        await api.GetReportResultOrderedAsync(new HttpPagination(page, limit));
    [HttpGet("coefficient/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetCoefficientResultOrdered(int page = 0, int limit = 0) =>
        await api.GetCoefficientResultOrderedAsync(new HttpPagination(page, limit));
    [HttpGet("dividend/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetDividendResultOrdered(int page = 0, int limit = 0) =>
        await api.GetDividendResultOrderedAsync(new HttpPagination(page, limit));


    [HttpGet("recalculate/")]
    public async Task<string> Recalculate() => await api.RecalculateAsync();
    [HttpGet("recalculate/{companyId}")]
    public async Task<string> Recalculate(string companyId)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1 ? await api.RecalculateAsync(companyIds) : await api.RecalculateAsync(companyId);
    }
    [HttpGet("recalculate/{companyId}/{year:int}")]
    public async Task<string> Recalculate(string companyId, int year)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.RecalculateAsync(new CompanyDateFilter<Rating>(companyIds, year))
            : await api.RecalculateAsync(new CompanyDateFilter<Rating>(companyId, year));
    }
    [HttpGet("recalculate/{companyId}/{year:int}/{month:int}")]
    public async Task<string> Recalculate(string companyId, int year, int month)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.RecalculateAsync(new CompanyDateFilter<Rating>(companyIds, year, month))
            : await api.RecalculateAsync(new CompanyDateFilter<Rating>(companyId, year, month));
    }
    [HttpGet("recalculate/{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<string> Recalculate(string companyId, int year, int month, int day)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.RecalculateAsync(new CompanyDateFilter<Rating>(HttpRequestFilterType.Equal, companyIds, year, month, day))
            : await api.RecalculateAsync(new CompanyDateFilter<Rating>(HttpRequestFilterType.Equal, companyId, year, month, day));
    }
    [HttpGet("recalculate/{year:int}")]
    public async Task<string> Recalculate(int year) =>
        await api.RecalculateAsync(new CompanyDateFilter<Rating>(year));
    [HttpGet("recalculate/{year:int}/{month:int}")]
    public async Task<string> Recalculate(int year, int month) =>
        await api.RecalculateAsync(new CompanyDateFilter<Rating>(year, month));
    [HttpGet("recalculate/{year:int}/{month:int}/{day:int}")]
    public async Task<string> Recalculate(int year, int month, int day) =>
        await api.RecalculateAsync(new CompanyDateFilter<Rating>(HttpRequestFilterType.Equal, year, month, day));
}