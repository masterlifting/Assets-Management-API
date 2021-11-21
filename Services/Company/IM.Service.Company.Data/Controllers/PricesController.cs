using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.Companies;

using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Services.DataServices.Prices;
using IM.Service.Company.Data.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Filters;
using static IM.Service.Common.Net.CommonEnums;

namespace IM.Service.Company.Data.Controllers;

[ApiController, Route("[controller]")]
public class PricesController : ControllerBase
{
    private readonly PricesDtoManager manager;
    private readonly PriceLoader loader;
    public PricesController(PricesDtoManager manager, PriceLoader loader)
    {
        this.manager = manager;
        this.loader = loader;
    }

    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.More, year, month, day), new(page, limit));

    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetLast(int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetLastAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.More, year, month, day), new(page, limit));

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> Get(string companyId, int year = 0, int month = 0, int day = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.More, companyId, year, month, day), new(page, limit));

    [HttpGet("{companyId}/{Year:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string companyId, int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(companyId, year), new(page, limit));

    [HttpGet("{companyId}/{Year:int}/{Month:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(string companyId, int year, int month, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(companyId, year, month), new(page, limit));

    [HttpGet("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
    public async Task<ResponseModel<PriceGetDto>> Get(string companyId, int year, int month, int day) =>
        await manager.GetAsync(companyId, new DateTime(year, month, day));


    [HttpGet("{Year:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(year), new(page, limit));

    [HttpGet("{Year:int}/{Month:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(year, month), new(page, limit));

    [HttpGet("{Year:int}/{Month:int}/{Day:int}")]
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetEqual(int year, int month, int day, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByDate<Price>(HttpRequestFilterType.Equal, year, month, day), new(page, limit));


    [HttpPost]
    public async Task<ResponseModel<string>> Post(PricePostDto model) => await manager.CreateAsync(model);
    [HttpPost("collection/")]
    public async Task<ResponseModel<string>> Post(IEnumerable<PricePostDto> models) => await manager.CreateAsync(models);

    [HttpPut("{companyId}/{Year:int}/{Month:int}/{Day:int}")]
    public async Task<ResponseModel<string>> Put(string companyId, int year, int month, int day, PricePutDto model) =>
        await manager.UpdateAsync(new PricePostDto
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
        var prices = await loader.DataSetAsync();
        return $"prices count: {prices.GroupBy(x => x.CompanyId).Count()} is loaded";
    }
}