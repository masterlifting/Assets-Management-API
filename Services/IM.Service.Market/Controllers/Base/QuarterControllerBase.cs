using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Market.Controllers.Base;

public class QuarterControllerBase<TEntity, TPost, TGet> : ControllerBase
    where TGet : class
    where TPost : class
    where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly RestApiWrite<TEntity, TPost> apiWrite;
    private readonly RestApiRead<TEntity, TGet> apiRead;

    public QuarterControllerBase(RestApiWrite<TEntity, TPost> apiWrite, RestApiRead<TEntity, TGet> apiRead)
    {
        this.apiWrite = apiWrite;
        this.apiRead = apiRead;
    }

    [HttpGet]
    public Task<ResponseModel<PaginatedModel<TGet>>> Gets(string? companyId, int? sourceId, int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
        apiRead.GetAsync(GetFilter(CompareType.More, companyId, sourceId, year, quarter), new(page, limit));
    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetLast(string? companyId, int? sourceId, int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
        await apiRead.GetLastAsync(GetFilter(CompareType.More, companyId, sourceId, year, quarter), new(page, limit));

    [HttpGet("{year:int}")]
    public Task<ResponseModel<PaginatedModel<TGet>>> Get(string? companyId, int? sourceId, int year, int page = 0, int limit = 0) =>
        apiRead.GetAsync(GetFilter(CompareType.Equal, companyId, sourceId, year), new(page, limit));
    [HttpGet("{year:int}/{quarter:int}")]
    public Task<ResponseModel<PaginatedModel<TGet>>> Get(string? companyId, int? sourceId, int year, int quarter, int page = 0, int limit = 0) =>
        apiRead.GetAsync(GetFilter(CompareType.Equal, companyId, sourceId, year, quarter), new(page, limit));
    

    [HttpPost]
    public async Task<IActionResult> Create(string companyId, int sourceId, TPost model)
    {
        var (error, _) = await apiWrite.CreateAsync(companyId, sourceId, model);

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpPost("collection/")]
    public async Task<IActionResult> Create(string companyId, int sourceId, IEnumerable<TPost> models)
    {
        var (error, _) = await apiWrite.CreateAsync(companyId, sourceId, models, new DataQuarterComparer<TEntity>());

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpPut("{year:int}/{quarter:int}")]
    public async Task<IActionResult> Update(string companyId, int sourceId, int year, int quarter, TPost model)
    {
        var id = Activator.CreateInstance<TEntity>();
        id.CompanyId = companyId;
        id.SourceId = (byte)sourceId;
        id.Year = year;
        id.Quarter = (byte)quarter;

        var (error, _) = await apiWrite.UpdateAsync(id, model);

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpDelete("{year:int}/{quarter:int}")]
    public async Task<IActionResult> Delete(string companyId, int sourceId, int year, int quarter)
    {
        var id = Activator.CreateInstance<TEntity>();
        id.CompanyId = companyId;
        id.SourceId = (byte)sourceId;
        id.Year = year;
        id.Quarter = (byte)quarter;

        var (error, _) = await apiWrite.DeleteAsync(id);

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpGet("load/")]
    public Task<string> Load(string companyId, int sourceId) => GetLoader(apiWrite, companyId, sourceId);


    private static QuarterFilter<TEntity> GetFilter(CompareType compareType, string? companyId, int? sourceId, int year = 0, int quarter = 0)
    {
        QuarterFilter<TEntity> filter;

        switch (companyId)
        {
            case null when sourceId is null:
                filter = quarter != 0
                        ? new(compareType, year, quarter)
                        : new(compareType, year);
                break;
            case null when true:
                filter = quarter != 0
                        ? new(compareType, (byte)sourceId, year, quarter)
                        : new(compareType, (byte)sourceId, year);
                break;
            default:
                {
                    var companyIds = companyId.Split(',');

                    filter = sourceId is null
                        ? companyIds.Length > 1
                            ? quarter != 0
                                    ? new(compareType, companyIds, year, quarter)
                                    : new(compareType, companyIds, year)
                            : quarter != 0
                                    ? new(compareType, companyId, year, quarter)
                                    : new(compareType, companyId, year)
                        : companyIds.Length > 1
                            ? quarter != 0
                                    ? new(compareType, companyIds, (byte)sourceId, year, quarter)
                                    : new(compareType, companyIds, (byte)sourceId, year)
                            : quarter != 0
                                    ? new(compareType, companyId, (byte)sourceId, year, quarter)
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