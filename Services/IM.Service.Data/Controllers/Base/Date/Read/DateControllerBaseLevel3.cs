using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.DataAccess.Filters;
using IM.Service.Data.Domain.Entities.Interfaces;
using IM.Service.Data.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;
using static IM.Service.Common.Net.Enums;
namespace IM.Service.Data.Controllers.Base.Date.Read;

public class DateControllerBaseLevel3<TEntity, TGet> : DateControllerBaseLevel2<TEntity, TGet> where TGet : class where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestMethodRead<TEntity, TGet> api;
    public DateControllerBaseLevel3(RestMethodRead<TEntity, TGet> api) : base(api) => this.api = api;

    [HttpGet("{sourceId:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetSource(int sourceId, int page = 0, int limit = 0) =>
        await api.GetAsync(new DateFilter<TEntity>((byte)sourceId), new(page, limit));
    [HttpGet("{sourceId:int}/{year:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetSource(int sourceId, int year, int page = 0, int limit = 0) =>
        await api.GetAsync(new DateFilter<TEntity>((byte)sourceId, year), new(page, limit));
    [HttpGet("{sourceId:int}/{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetSource(int sourceId, int year, int month, int page = 0, int limit = 0) =>
        await api.GetAsync(new DateFilter<TEntity>(HttpRequestFilterType.Equal, (byte)sourceId, year, month), new(page, limit));
    [HttpGet("{sourceId:int}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetSource(int sourceId, int year, int month, int day, int page = 0, int limit = 0) =>
        await api.GetAsync(new DateFilter<TEntity>(HttpRequestFilterType.Equal, (byte)sourceId, year, month, day), new(page, limit));
}
