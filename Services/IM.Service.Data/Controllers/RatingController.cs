using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Data.Controllers.Base.Date.Read;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Models.Api.Http;
using IM.Service.Data.Services.RestApi;
using IM.Service.Data.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Data.Controllers;

[ApiController, Route("[controller]")]
public class RatingController : DateControllerBaseLevel2<Rating, RatingGetDto>
{
    private readonly RatingApi api;
    public RatingController(
        RatingApi api,
        RestMethodRead<Rating, RatingGetDto> apiRead
    ) : base(apiRead) => this.api = api;

    [HttpGet("{place:int}")]
    public async Task<ResponseModel<RatingGetDto>> Get(int place) => await api.GetAsync(place);

    [HttpGet("price/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetPriceResultOrdered(int page = 0, int limit = 0) =>
        await api.GetPriceResultOrderedAsync(new HttpPagination(page, limit));
    [HttpGet("report/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetReportResultOrdered(int page = 0, int limit = 0) =>
        await api.GetReportResultOrderedAsync(new HttpPagination(page, limit));
    [HttpGet("coefficient/")]
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetCoefficientResultOrdered(int page = 0, int limit = 0) =>
        await api.GetCoefficientResultOrderedAsync(new HttpPagination(page, limit));


    [HttpGet("recalculate/")]
    public async Task<string> Recalculate() => await api.RecalculateAsync();

    [HttpGet("recalculate/{companyId}")]
    public async Task<string> Recalculate(string companyId)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1 ? await api.RecalculateAsync(companyIds) : await api.RecalculateAsync(companyId);
    }

    //[HttpGet("recalculate/{companyId}/{year:int}")]
    //public async Task<string> Recalculate(string companyId, int year)
    //{
    //    var companyIds = companyId.Split(',');
    //    return companyIds.Length > 1
    //        ? await api.RecalculateAsync(new DateFilter<Rating>(companyIds, year))
    //        : await api.RecalculateAsync(new DateFilter<Rating>(companyId, year));
    //}

    //[HttpGet("recalculate/{companyId}/{year:int}/{month:int}")]
    //public async Task<string> Recalculate(string companyId, int year, int month)
    //{
    //    var companyIds = companyId.Split(',');
    //    return companyIds.Length > 1
    //        ? await api.RecalculateAsync(new DateFilter<Rating>(companyIds, year, month))
    //        : await api.RecalculateAsync(new DateFilter<Rating>(companyId, year, month));
    //}

    //[HttpGet("recalculate/{companyId}/{year:int}/{month:int}/{day:int}")]
    //public async Task<string> Recalculate(string companyId, int year, int month, int day)
    //{
    //    var companyIds = companyId.Split(',');
    //    return companyIds.Length > 1
    //        ? await api.RecalculateAsync(new DateFilter<Rating>(HttpRequestFilterType.Equal, companyIds, year, month, day))
    //        : await api.RecalculateAsync(new DateFilter<Rating>(HttpRequestFilterType.Equal, companyId, year, month, day));
    //}

    //[HttpGet("recalculate/{year:int}")]
    //public async Task<string> Recalculate(int year) =>
    //    await api.RecalculateAsync(new DateFilter<Rating>(year));

    //[HttpGet("recalculate/{year:int}/{month:int}")]
    //public async Task<string> Recalculate(int year, int month) =>
    //    await api.RecalculateAsync(new DateFilter<Rating>(year, month));

    //[HttpGet("recalculate/{year:int}/{month:int}/{day:int}")]
    //public async Task<string> Recalculate(int year, int month, int day) =>
    //    await api.RecalculateAsync(new DateFilter<Rating>(HttpRequestFilterType.Equal, year, month, day));
}