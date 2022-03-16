using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.MarketData.Domain.DataAccess.Filters;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;
using static IM.Service.Common.Net.Enums;

namespace IM.Service.MarketData.Controllers.Base.Date.Read;

public class DateControllerBaseLevel1<TEntity, TGet> : ControllerBase where TGet : class where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestMethodRead<TEntity, TGet> api;
    public DateControllerBaseLevel1(RestMethodRead<TEntity, TGet> api) => this.api = api;

    [HttpGet("{companyId}/{sourceId:int}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> Get(string companyId, int sourceId, int year, int month, int day, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new DateFilter<TEntity>(HttpRequestFilterType.Equal, companyIds, (byte)sourceId, year, month, day), new(page, limit))
            : await api.GetAsync(new DateFilter<TEntity>(HttpRequestFilterType.Equal, companyId, (byte)sourceId, year, month, day), new(page, limit));
    }

    #region Get More
    [HttpGet]
    public async Task<ResponseModel<PaginatedModel<TGet>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await api.GetAsync(new DateFilter<TEntity>(HttpRequestFilterType.More, year, month, day), new(page, limit));
    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await api.GetLastAsync(new DateFilter<TEntity>(HttpRequestFilterType.More, year, month, day), new(page, limit));
    #endregion

    #region Get Equal
    [HttpGet("{year:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetPeriod(int year, int page = 0, int limit = 0) =>
        await api.GetAsync(new DateFilter<TEntity>(year), new(page, limit));
    [HttpGet("{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetPeriod(int year, int month, int page = 0, int limit = 0) =>
        await api.GetAsync(new DateFilter<TEntity>(year, month), new(page, limit));
    [HttpGet("{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetPeriod(int year, int month, int day, int page = 0, int limit = 0) =>
        await api.GetAsync(new DateFilter<TEntity>(HttpRequestFilterType.Equal, year, month, day), new(page, limit));
    #endregion
}