using System.Collections.Generic;
using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using IM.Service.Market.Models.Dto;
using IM.Service.Market.Services.DtoServices;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly CompanyDtoManager manager;
    public CompaniesController(CompanyDtoManager manager) => this.manager = manager;

    public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> Get(int page = 0, int limit = 0) =>
        await manager.GetAsync((HttpPagination) new(page, limit));

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<CompanyGetDto>> Get(string companyId) => await manager.GetAsync(companyId);

    [HttpPost]
    public async Task<ResponseModel<string>> Post(CompanyPostDto model) => await manager.CreateAsync(model);

    [HttpPost("collection/")]
    public async Task<ResponseModel<string>> Post(IEnumerable<CompanyPostDto> models) => await manager.CreateAsync(models);

    [HttpPut("{companyId}")]
    public async Task<ResponseModel<string>> Put(string companyId, CompanyPutDto model) => await manager.UpdateAsync(companyId, model);
    
    [HttpPut("collection/")]
    public async Task<ResponseModel<string>> Put(IEnumerable<CompanyPostDto> models) => await manager.UpdateAsync(models);

    [HttpDelete("{companyId}")]
    public async Task<ResponseModel<string>> Delete(string companyId) => await manager.DeleteAsync(companyId);

    [HttpGet("sync/")]
    public async Task<string> Sync() => await manager.SyncAsync();
}