using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Market.Controllers.Base;

public class DateControllerBase<TEntity, TPost, TGet> : ControllerBase
    where TGet : class
    where TPost : class
    where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestApiWrite<TEntity, TPost> apiWrite;
    private readonly RestApiRead<TEntity, TGet> apiRead;

    public DateControllerBase(RestApiWrite<TEntity, TPost> apiWrite, RestApiRead<TEntity, TGet> apiRead)
    {
        this.apiWrite = apiWrite;
        this.apiRead = apiRead;
    }

    [HttpGet]
    public Task<ResponseModel<PaginatedModel<TGet>>> Gets(string? companyId, int? sourceId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        apiRead.GetAsync(GetFilter(CompareType.More, companyId, sourceId, year, month, day), new(page, limit));
    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetLast(string? companyId, int? sourceId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await apiRead.GetLastAsync(GetFilter(CompareType.More, companyId, sourceId, year, month, day), new(page, limit));

    [HttpGet("{year:int}")]
    public Task<ResponseModel<PaginatedModel<TGet>>> Get(string? companyId, int? sourceId, int year, int page = 0, int limit = 0) =>
        apiRead.GetAsync(GetFilter(CompareType.Equal, companyId, sourceId, year), new(page, limit));
    [HttpGet("{year:int}/{month:int}")]
    public Task<ResponseModel<PaginatedModel<TGet>>> Get(string? companyId, int? sourceId, int year, int month, int page = 0, int limit = 0) =>
        apiRead.GetAsync(GetFilter(CompareType.Equal, companyId, sourceId, year, month), new(page, limit));
    [HttpGet("{year:int}/{month:int}/{day:int}")]
    public Task<ResponseModel<PaginatedModel<TGet>>> Get(string? companyId, int? sourceId, int year, int month, int day, int page = 0, int limit = 0) =>
        apiRead.GetAsync(GetFilter(CompareType.Equal, companyId, sourceId, year, month, day), new(page, limit));

    [HttpPost]
    public async Task<IActionResult> Create(string companyId, int sourceId, TPost model)
    {
        var (error, _) = await apiWrite.CreateAsync(companyId, sourceId, model);

        return error is null ? Ok() : BadRequest(error);
    }
    [HttpPost("collection/")]
    public async Task<IActionResult> Create(string companyId, int sourceId, IEnumerable<TPost> models)
    {
        var (error, _) = await apiWrite.CreateAsync(companyId, sourceId, models, new DataDateComparer<TEntity>());

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpPut("{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Update(string companyId, int sourceId, int year, int month, int day, TPost model)
    {
        var id = Activator.CreateInstance<TEntity>();
        id.CompanyId = companyId;
        id.SourceId = (byte)sourceId;
        id.Date = new DateOnly(year, month, day);

        var (error, _) = await apiWrite.UpdateAsync(id, model);

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpDelete("{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Delete(string companyId, int sourceId, int year, int month, int day)
    {
        var id = Activator.CreateInstance<TEntity>();
        id.CompanyId = companyId;
        id.SourceId = (byte)sourceId;
        id.Date = new DateOnly(year, month, day);

        var (error, _) = await apiWrite.DeleteAsync(id);

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpGet("load/")]
    public Task<string> Load(string? companyId, int? sourceId) => GetLoader(apiWrite, companyId, sourceId);


    private static DateFilter<TEntity> GetFilter(CompareType compareType, string? companyId, int? sourceId, int year = 0, int month = 0, int day = 0)
    {
        DateFilter<TEntity> filter;

        switch (companyId)
        {
            case null when sourceId is null:
                filter = day != 0
                    ? new(compareType, year, month, day)
                    : month != 0
                        ? new(compareType, year, month)
                        : new(compareType, year);
                break;
            case null when true:
                filter = day != 0
                    ? new(compareType, (byte)sourceId, year, month, day)
                    : month != 0
                        ? new(compareType, (byte)sourceId, year, month)
                        : new(compareType, (byte)sourceId, year);
                break;
            default:
                {
                    var companyIds = companyId.Split(',');

                    filter = sourceId is null
                        ? companyIds.Length > 1
                            ? day != 0
                                ? new(compareType, companyIds, year, month, day)
                                : month != 0
                                    ? new(compareType, companyIds, year, month)
                                    : new(compareType, companyIds, year)
                            : day != 0
                                ? new(compareType, companyId, year, month, day)
                                : month != 0
                                    ? new(compareType, companyId, year, month)
                                    : new(compareType, companyId, year)
                        : companyIds.Length > 1
                            ? day != 0
                                ? new(compareType, companyIds, (byte)sourceId, year, month, day)
                                : month != 0
                                    ? new(compareType, companyIds, (byte)sourceId, year, month)
                                    : new(compareType, companyIds, (byte)sourceId, year)
                            : day != 0
                                ? new(compareType, companyId, (byte)sourceId, year, month, day)
                                : month != 0
                                    ? new(compareType, companyId, (byte)sourceId, year, month)
                                    : new(compareType, companyId, (byte)sourceId, year);

                    break;
                }
        }

        return filter;
    }
    private static Task<string> GetLoader(RestApiWrite<TEntity, TPost> api, string? companyId, int? sourceId)
    {
        Task<string> loader;

        switch (companyId)
        {
            case null when sourceId is null:
                loader = api.LoadAsync();
                break;
            case null when true:
                loader = api.LoadAsync((byte)sourceId.Value);
                break;
            default:
                {
                    var companyIds = companyId.Split(',');

                    loader = sourceId is null
                        ? companyIds.Length > 1
                            ? Task.FromResult("Загрузка по выбранным компаниям не предусмотрена")
                            : api.LoadAsync(companyId)
                        : companyIds.Length > 1
                            ? Task.FromResult("Загрузка по выбранным компаниям не предусмотрена")
                            : api.LoadAsync(companyId, (byte)sourceId.Value);
                    break;
                }
        }

        return loader;
    }
}