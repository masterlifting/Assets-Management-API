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
public class PricesController : ControllerBase
{
    private readonly PricesDtoManager manager;
    public PricesController(PricesDtoManager manager) => this.manager = manager;

    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.More, year, month, day), new(page, limit));

    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetLastAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.More, year, month, day), new(page, limit));


    [HttpGet("{companyId}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(string companyId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
                ? await manager.GetAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.More, companyIds, year, month, day), new(page, limit))
                : await manager.GetAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.More, companyId, year, month, day), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string companyId, int year, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
                ? await manager.GetAsync(new CompanyDataFilterByDate<Price>(companyIds, year), new(page, limit))
                : await manager.GetAsync(new CompanyDataFilterByDate<Price>(companyId, year), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string companyId, int year, int month, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Length > 1
                ? await manager.GetAsync(new CompanyDataFilterByDate<Price>(companyIds, year, month), new(page, limit))
                : await manager.GetAsync(new CompanyDataFilterByDate<Price>(companyId, year, month), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(string companyId, int year, int month, int day)
    {
        var companyIds = companyId.Split(',');
        
        if (companyIds.Length == 1)
        {
            var result = await manager.GetAsync(companyId, new DateOnly(year, month, day));
            return result.Errors.Any()
                ? new() { Errors = result.Errors }
                : new() { Data = new() { Count = 1, Items = new[] { result.Data! } } };
        }

        List<ResponseModel<PriceGetDto>> results = new(companyIds.Length);

        foreach (var id in companyIds)
            results.Add(await manager.GetAsync(id, new DateOnly(year, month, day)));

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
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(year), new(page, limit));

    [HttpGet("{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(year, month), new(page, limit));

    [HttpGet("{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


    [HttpPost]
    public async Task<ResponseModel<string>> Post(PricePostDto model) => await manager.CreateAsync(model);
    [HttpPost("collection/")]
    public async Task<ResponseModel<string>> Post(IEnumerable<PricePostDto> models) => await manager.CreateAsync(models);

    [HttpPut("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<string>> Put(string companyId, int year, int month, int day, PricePutDto model) =>
        await manager.UpdateAsync(new PricePostDto
        {
            CompanyId = companyId,
            Date = new DateOnly(year, month, day),
            SourceType = model.SourceType,
            Value = model.Value
        });
    [HttpDelete("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<string>> Delete(string companyId, int year, int month, int day) =>
        await manager.DeleteAsync(companyId, new DateTime(year, month, day));

    
    [HttpGet("load/")]
    public string Load() => manager.Load();
    [HttpGet("load/{companyId}")]
    public string Load(string companyId) => manager.Load(companyId);
}