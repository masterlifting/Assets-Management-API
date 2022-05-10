using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.Http.Common;

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
    public async Task<IActionResult> Gets(string? companyId, int? sourceId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0)
    {
        try
        {
            var result = await apiRead.GetAsync(DateFilter<TEntity>.GetFilter(CompareType.More, companyId, sourceId, year, month, day), new(page, limit));
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("last/")]
    public async Task<IActionResult> GetLast(string? companyId, int? sourceId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0)
    {
        try
        {
            var result = await apiRead.GetLastAsync(DateFilter<TEntity>.GetFilter(CompareType.More, companyId, sourceId, year, month, day), new(page, limit));
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
            var result = await apiRead.GetAsync(DateFilter<TEntity>.GetFilter(CompareType.Equal, companyId, sourceId, year), new(page, limit));
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{year:int}/{month:int}")]
    public async Task<IActionResult> Get(string? companyId, int? sourceId, int year, int month, int page = 0, int limit = 0)
    {
        try
        {
            var result = await apiRead.GetAsync(DateFilter<TEntity>.GetFilter(CompareType.Equal, companyId, sourceId, year, month), new(page, limit));
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Get(string? companyId, int? sourceId, int year, int month, int day, int page = 0, int limit = 0)
    {
        try
        {
            var result = await apiRead.GetAsync(DateFilter<TEntity>.GetFilter(CompareType.Equal, companyId, sourceId, year, month, day), new(page, limit));
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
            var result = await apiWrite.CreateAsync(companyId, sourceId, models, new DataDateComparer<TEntity>());
            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Update(string companyId, int sourceId, int year, int month, int day, TPost model)
    {
        try
        {
            var id = Activator.CreateInstance<TEntity>();
            id.CompanyId = companyId;
            id.SourceId = (byte)sourceId;
            id.Date = new DateOnly(year, month, day);

            var result = await apiWrite.UpdateAsync(id, model);

            return Ok(result);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{year:int}")]
    public async Task<IActionResult> Delete(string? companyId, int? sourceId, int year)
    {
        try
        {
            return Ok(await apiWrite.DeleteAsync(DateFilter<TEntity>.GetFilter(CompareType.Equal, companyId, sourceId, year)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpDelete("{year:int}/{month:int}")]
    public async Task<IActionResult> Delete(string? companyId, int? sourceId, int year, int month)
    {
        try
        {
            return Ok(await apiWrite.DeleteAsync(DateFilter<TEntity>.GetFilter(CompareType.Equal, companyId, sourceId, year, month)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpDelete("{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Delete(string? companyId, int? sourceId, int year, int month, int day)
    {
        try
        {
            return Ok(await apiWrite.DeleteAsync(DateFilter<TEntity>.GetFilter(CompareType.Equal, companyId, sourceId, year, month, day)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }


    [HttpGet("load/")]
    public async Task<IActionResult> Load(string? companyId, int? sourceId)
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