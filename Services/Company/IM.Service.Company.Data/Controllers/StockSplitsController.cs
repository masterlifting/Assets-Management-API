using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Services.DataServices.StockSplits;
using IM.Service.Company.Data.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Common.Net.CommonEnums;

namespace IM.Service.Company.Data.Controllers;

[ApiController, Route("[controller]")]
public class StockSplitsController : ControllerBase
{
    private readonly StockSplitsDtoManager manager;
    private readonly StockSplitLoader loader;
    public StockSplitsController(StockSplitsDtoManager manager, StockSplitLoader loader)
    {
        this.manager = manager;
        this.loader = loader;
    }

    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.More, year, month, day), new(page, limit));

    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetLastAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.More, year, month, day), new(page, limit));

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> Get(string companyId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.More, companyId, year, month, day), new(page, limit));

    [HttpGet("{companyId}/{Year:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string companyId, int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(companyId, year), new(page, limit));

    [HttpGet("{companyId}/{Year:int}/{Month:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(string companyId, int year, int month, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(companyId, year, month), new(page, limit));

    [HttpGet("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
    public async Task<ResponseModel<StockSplitGetDto>> Get(string companyId, int year, int month, int day) =>
        await manager.GetAsync(companyId, new DateTime(year, month, day));


    [HttpGet("{Year:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(year), new(page, limit));

    [HttpGet("{Year:int}/{Month:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(year, month), new(page, limit));

    [HttpGet("{Year:int}/{Month:int}/{Day:int}")]
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<StockSplit>(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


    [HttpPost]
    public async Task<ResponseModel<string>> Post(StockSplitPostDto model) => await manager.CreateAsync(model);
    [HttpPost("collection/")]
    public async Task<ResponseModel<string>> Post(IEnumerable<StockSplitPostDto> models) => await manager.CreateAsync(models);

    [HttpPut("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
    public async Task<ResponseModel<string>> Put(string companyId, int year, int month, int day, StockSplitPutDto model) =>
        await manager.UpdateAsync(new StockSplitPostDto
        {
            CompanyId = companyId,
            Date = new DateTime(year, month, day),
            SourceType = model.SourceType,
            Value = model.Value
        });
    [HttpDelete("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
    public async Task<ResponseModel<string>> Delete(string companyId, int year, int month, int day) =>
        await manager.DeleteAsync(companyId, new DateTime(year, month, day));

    [HttpPost("load/")]
    public async Task<string> Load()
    {
        var splits = await loader.DataSetAsync();
        return $"Loaded stock splits count: {splits.Length}";
    }
}