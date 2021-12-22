using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Services.DataServices.StockVolumes;
using IM.Service.Company.Data.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.CommonEnums;

namespace IM.Service.Company.Data.Controllers;

[ApiController, Route("[controller]")]
public class StockVolumesController : ControllerBase
{
    private readonly StockVolumesDtoManager manager;
    private readonly StockVolumeLoader loader;
    public StockVolumesController(StockVolumesDtoManager manager, StockVolumeLoader loader)
    {
        this.manager = manager;
        this.loader = loader;
    }

    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.More, year, month, day), new(page, limit));

    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetLastAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.More, year, month, day), new(page, limit));


    [HttpGet("{companyId}")]
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> Get(string companyId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');

        return companyIds.Any()
            ? await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.More, companyIds, year, month, day), new(page, limit))
            : await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.More, companyId, year, month, day), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}")]
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(string companyId, int year, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Any()
            ? await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(companyIds, year), new(page, limit))
            : await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(companyId, year), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(string companyId, int year, int month, int page = 0, int limit = 0)
    {
        var companyIds = companyId.Split(',');
        return companyIds.Any()
            ? await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(companyIds, year, month), new(page, limit))
            : await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(companyId, year, month), new(page, limit));
    }

    [HttpGet("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> Get(string companyId, int year, int month, int day)
    {
        var companyIds = companyId.Split(',');

        if (!companyIds.Any())
        {
            var result = await manager.GetAsync(companyId, new DateTime(year, month, day));
            return result.Errors.Any()
                ? new() { Errors = result.Errors }
                : new() { Data = new() { Count = 1, Items = new[] { result.Data! } } };
        }

        List<ResponseModel<StockVolumeGetDto>> results = new(companyIds.Length);

        foreach (var Id in companyIds)
            results.Add(await manager.GetAsync(Id, new DateTime(year, month, day)));

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
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(year), new(page, limit));

    [HttpGet("{year:int}/{month:int}")]
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(year, month), new(page, limit));

    [HttpGet("{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockVolume>(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


    [HttpPost]
    public async Task<ResponseModel<string>> Post(StockVolumePostDto model) => await manager.CreateAsync(model);
    [HttpPost("collection/")]
    public async Task<ResponseModel<string>> Post(IEnumerable<StockVolumePostDto> models) => await manager.CreateAsync(models);

    [HttpPut("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<string>> Put(string companyId, int year, int month, int day, StockVolumePutDto model) =>
        await manager.UpdateAsync(new StockVolumePostDto
        {
            CompanyId = companyId,
            Date = new DateTime(year, month, day),
            SourceType = model.SourceType,
            Value = model.Value
        });
    [HttpDelete("{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<ResponseModel<string>> Delete(string companyId, int year, int month, int day) =>
        await manager.DeleteAsync(companyId, new DateTime(year, month, day));

    [HttpPost("load/")]
    public async Task<string> Load()
    {
        var volumes = await loader.DataSetAsync();
        return $"Loaded stock volumes count: {volumes.Length}";
    }
}