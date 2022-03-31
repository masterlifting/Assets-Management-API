using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;
namespace IM.Service.Market.Controllers.Base.Date.Read;

public class DateControllerBaseLevel4<TEntity, TGet> : DateControllerBaseLevel3<TEntity, TGet> where TGet : class where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestApiRead<TEntity, TGet> api;
    public DateControllerBaseLevel4(RestApiRead<TEntity, TGet> api) : base(api) => this.api = api;

    [HttpGet("{companyId}/{sourceId:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompanySource(string companyId, int sourceId, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new DateFilter<TEntity>(companyIds,(byte)sourceId), new(page, limit))
            : await api.GetAsync(new DateFilter<TEntity>(companyId, (byte)sourceId), new(page, limit));
    }
    [HttpGet("{companyId}/{sourceId:int}/{year:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompanySource(string companyId, int sourceId, int year, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new DateFilter<TEntity>(companyIds, (byte)sourceId, year), new(page, limit))
            : await api.GetAsync(new DateFilter<TEntity>(companyId, (byte)sourceId, year), new(page, limit));
    }
    [HttpGet("{companyId}/{sourceId:int}/{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetCompanySource(string companyId, int sourceId, int year, int month, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
            ? await api.GetAsync(new DateFilter<TEntity>(companyIds, (byte)sourceId, year, month), new(page, limit))
            : await api.GetAsync(new DateFilter<TEntity>(companyId, (byte)sourceId, year, month), new(page, limit));
    }
}