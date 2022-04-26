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
    public async Task<IActionResult> Gets(string? companyId, int? sourceId, int year = 0, int quarter = 0, int page = 0, int limit = 0)
    {
        try
        {
            var result = await apiRead.GetAsync(QuarterFilter<TEntity>.GetFilter(CompareType.More, companyId, sourceId, year, quarter), new(page, limit));
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("last/")]
    public async Task<IActionResult> GetLast(string? companyId, int? sourceId, int year = 0, int quarter = 0, int page = 0, int limit = 0)
    {
        try
        {
            var result = await apiRead.GetLastAsync(QuarterFilter<TEntity>.GetFilter(CompareType.More, companyId, sourceId, year, quarter), new(page, limit));
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{year:int}")]
    public async Task<IActionResult> Get(string? companyId, int? sourceId, int year, int page = 0, int limit = 0)
    {
        try
        {
            var result = await apiRead.GetAsync(QuarterFilter<TEntity>.GetFilter(CompareType.Equal, companyId, sourceId, year), new(page, limit));
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{year:int}/{quarter:int}")]
    public async Task<IActionResult> Get(string? companyId, int? sourceId, int year, int quarter, int page = 0, int limit = 0)
    {
        try
        {
            var result = await apiRead.GetAsync(QuarterFilter<TEntity>.GetFilter(CompareType.Equal, companyId, sourceId, year, quarter), new(page, limit));
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }


    [HttpPost]
    public async Task<IActionResult> Create(string companyId, int sourceId, TPost model)
    {
        try
        {
            var result = await apiWrite.CreateAsync(companyId, sourceId, model);
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("collection/")]
    public async Task<IActionResult> Create(string companyId, int sourceId, IEnumerable<TPost> models)
    {
        try
        {
            var result = await apiWrite.CreateAsync(companyId, sourceId, models, new DataQuarterComparer<TEntity>());
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{year:int}/{quarter:int}")]
    public async Task<IActionResult> Update(string companyId, int sourceId, int year, int quarter, TPost model)
    {
        try
        {
            var id = Activator.CreateInstance<TEntity>();
            id.CompanyId = companyId;
            id.SourceId = (byte)sourceId;
            id.Year = year;
            id.Quarter = (byte)quarter;

            var result = await apiWrite.UpdateAsync(id, model);

            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{year:int}/{quarter:int}")]
    public async Task<IActionResult> Delete(string companyId, int sourceId, int year, int quarter)
    {
        try
        {
            var id = Activator.CreateInstance<TEntity>();
            id.CompanyId = companyId;
            id.SourceId = (byte)sourceId;
            id.Year = year;
            id.Quarter = (byte)quarter;

            var result = await apiWrite.DeleteAsync(id);

            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("load/")]
    public async Task<IActionResult> Load(string companyId, int sourceId)
    {
        try
        {
            return Ok(await GetLoaderAsync(apiWrite, companyId, sourceId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }


    private static Task<string> GetLoaderAsync(RestApiWrite<TEntity, TPost> api, string? companyId, int? sourceId)
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