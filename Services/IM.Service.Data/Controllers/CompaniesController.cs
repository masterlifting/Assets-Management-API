using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Data.Models.Api.Http;
using IM.Service.Data.Services.RestApi;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Data.Controllers;

[ApiController, Route("[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly CompanyApi api;
    public CompaniesController(CompanyApi api) => this.api = api;

    public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> Get(int page = 0, int limit = 0) =>
        await api.GetAsync((HttpPagination) new(page, limit));

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<CompanyGetDto>> Get(string companyId) => await api.GetAsync(companyId);

    [HttpPost]
    public async Task<ResponseModel<string>> Post(CompanyPostDto model) => await api.CreateAsync(model);

    [HttpPost("collection/")]
    public async Task<ResponseModel<string>> Post(IEnumerable<CompanyPostDto> models) => await api.CreateAsync(models);

    [HttpPut("{companyId}")]
    public async Task<ResponseModel<string>> Put(string companyId, CompanyPutDto model) => await api.UpdateAsync(companyId, model);
    
    [HttpPut("collection/")]
    public async Task<ResponseModel<string>> Put(IEnumerable<CompanyPostDto> models) => await api.UpdateAsync(models);

    [HttpDelete("{companyId}")]
    public async Task<ResponseModel<string>> Delete(string companyId) => await api.DeleteAsync(companyId);

    [HttpGet("sync/")]
    public async Task<string> Sync() => await api.SyncAsync();
}