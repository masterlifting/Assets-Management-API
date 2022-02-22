using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService.Filters;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.Services.DtoServices;
using static IM.Service.Common.Net.Enums;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class StockSplitsController : ControllerBase
{
    private readonly StockSplitsDtoManager manager;
    public StockSplitsController(StockSplitsDtoManager manager) => this.manager = manager;

    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.More, year, month, day), new(page, limit));

    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetLastAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.More, year, month, day), new(page, limit));


    [HttpGet("{companyId}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(string companyId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
                ? await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.More, companyIds, year, month, day), new(page, limit))
                : await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.More, companyId, year, month, day), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string companyId, int year, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
                ? await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(companyIds, year), new(page, limit))
                : await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(companyId, year), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string companyId, int year, int month, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
                ? await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(companyIds, year, month), new(page, limit))
                : await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(companyId, year, month), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(string companyId, int year, int month, int day)
    {
        var companyIds = companyId.Split(',');
        if (companyIds.Length == 1)
        {
            var result = await manager.GetAsync(companyId, new DateTime(year, month, day));
            return result.Errors.Any()
                ? new() { Errors = result.Errors }
                : new() { Data = new() { Count = 1, Items = new[] { result.Data! } } };
        }

        List<ResponseModel<StockSplitGetDto>> results = new(companyIds.Length);

        foreach (var id in companyIds)
            results.Add(await manager.GetAsync(id, new DateTime(year, month, day)));

        var resultWithoutErrors = results.Where(x => !x.Errors.Any()).ToArray();
        return new()
        {
            Errors = results.SelectMany(x => x.Errors).ToArray(),
            Data = new()
            {
                Count = resultWithoutErrors.Length,
                Items = resultWithoutErrors.Select(x => x.Data!).ToArray()
            }
        };
    }


    [HttpGet("{year:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(year), new(page, limit));

    [HttpGet("{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(year, month), new(page, limit));

    [HttpGet("{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


    [HttpPost]
    public async Task<ResponseModel<string>> Post(StockSplitPostDto model) => await manager.CreateAsync(model);
    [HttpPost("collection/")]
    public async Task<ResponseModel<string>> Post(IEnumerable<StockSplitPostDto> models) => await manager.CreateAsync(models);

    [HttpPut("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<string>> Put(string companyId, int year, int month, int day, StockSplitPutDto model) =>
        await manager.UpdateAsync(new StockSplitPostDto
        {
            CompanyId = companyId,
            Date = new DateOnly(year, month, day),
            SourceType = model.SourceType,
            Value = model.Value
        });
    [HttpDelete("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<string>> Delete(string companyId, int year, int month, int day) =>
        await manager.DeleteAsync(companyId, new DateOnly(year, month, day));

    [HttpGet("load/")]
    public string Load() => manager.Load();
    [HttpGet("load/{companyId}")]
    public string Load(string companyId) => manager.Load(companyId);
}