using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.MarketData.Domain.DataAccess.Filters;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;
using static IM.Service.Common.Net.Enums;

namespace IM.Service.MarketData.Controllers.Base.Quarter.Read;

public class QuarterControllerBaseLevel2<TEntity, TGet> : QuarterControllerBaseLevel1<TEntity, TGet> where TGet : class where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly RestMethodRead<TEntity, TGet> api;
    public QuarterControllerBaseLevel2(RestMethodRead<TEntity, TGet> api) : base(api) => this.api = api;

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompany(string companyId, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new QuarterFilter<TEntity>(companyIds), new(page, limit))
            : await api.GetAsync(new QuarterFilter<TEntity>(companyId), new(page, limit));
    }
    [HttpGet("{companyId}/{year:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompany(string companyId, int year, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new QuarterFilter<TEntity>(companyIds, year), new(page, limit))
            : await api.GetAsync(new QuarterFilter<TEntity>(companyId, year), new(page, limit));
    }
    [HttpGet("{companyId}/{year:int}/{quarter:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompany(string companyId, int year, int quarter, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new QuarterFilter<TEntity>(HttpRequestFilterType.Equal, companyIds, year, quarter), new(page, limit))
            : await api.GetAsync(new QuarterFilter<TEntity>(HttpRequestFilterType.Equal, companyId, year, quarter), new(page, limit));
    }
}