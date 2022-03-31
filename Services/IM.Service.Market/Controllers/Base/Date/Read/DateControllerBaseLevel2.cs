using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;
using static IM.Service.Common.Net.Enums;

namespace IM.Service.Market.Controllers.Base.Date.Read;

public class DateControllerBaseLevel2<TEntity, TGet> : DateControllerBaseLevel1<TEntity, TGet> where TGet : class where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestApiRead<TEntity, TGet> api;
    public DateControllerBaseLevel2(RestApiRead<TEntity, TGet> api) : base(api) => this.api = api;

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompany(string companyId, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new DateFilter<TEntity>(companyIds), new(page, limit))
            : await api.GetAsync(new DateFilter<TEntity>(companyId), new(page, limit));
    }
    [HttpGet("{companyId}/{year:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompany(string companyId, int year, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new DateFilter<TEntity>(companyIds, year), new(page, limit))
            : await api.GetAsync(new DateFilter<TEntity>(companyId, year), new(page, limit));
    }
    [HttpGet("{companyId}/{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompany(string companyId, int year, int month, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new DateFilter<TEntity>(companyIds, year, month), new(page, limit))
            : await api.GetAsync(new DateFilter<TEntity>(companyId, year, month), new(page, limit));
    }
    [HttpGet("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompany(string companyId, int year, int month, int day, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new DateFilter<TEntity>(HttpRequestFilterType.Equal, companyIds, year, month, day), new(page, limit))
            : await api.GetAsync(new DateFilter<TEntity>(HttpRequestFilterType.Equal, companyId, year, month, day), new(page, limit));
    }
}