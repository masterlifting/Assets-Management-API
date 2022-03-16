using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.MarketData.Domain.DataAccess.Filters;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;
using static IM.Service.Common.Net.Enums;

namespace IM.Service.MarketData.Controllers.Base.Quarter.Read;

public class QuarterControllerBaseLevel1<TEntity, TGet> : ControllerBase where TGet : class where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly RestMethodRead<TEntity, TGet> api;
    public QuarterControllerBaseLevel1(RestMethodRead<TEntity, TGet> api) => this.api = api;

    [HttpGet("{companyId}/{sourceId:int}/{year:int}/{quarter:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> Get(string companyId, int sourceId, int year, int quarter, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new QuarterFilter<TEntity>(HttpRequestFilterType.Equal, companyIds, (byte)sourceId, year, quarter), new(page, limit))
            : await api.GetAsync(new QuarterFilter<TEntity>(HttpRequestFilterType.Equal, companyId, (byte)sourceId, year, quarter), new(page, limit));
    }

    #region Get More
    [HttpGet]
    public async Task<ResponseModel<PaginatedModel<TGet>>> Get(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
        await api.GetAsync(new QuarterFilter<TEntity>(HttpRequestFilterType.More, year, quarter), new(page, limit));
    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetLast(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
        await api.GetLastAsync(new QuarterFilter<TEntity>(HttpRequestFilterType.More, year, quarter), new(page, limit));
    #endregion

    #region Get Equal
    [HttpGet("{year:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetPeriod(int year, int page = 0, int limit = 0) =>
        await api.GetAsync(new QuarterFilter<TEntity>(year), new(page, limit));
    [HttpGet("{year:int}/{quarter:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetPeriod(int year, int quarter, int page = 0, int limit = 0) =>
        await api.GetAsync(new QuarterFilter<TEntity>(HttpRequestFilterType.Equal, year, quarter), new(page, limit));
    #endregion
}