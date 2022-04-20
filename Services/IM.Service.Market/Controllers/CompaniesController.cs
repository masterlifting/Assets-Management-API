using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly CompanyRestApi api;
    private readonly CompanySourceRestApi csApi;

    public CompaniesController(CompanyRestApi api, CompanySourceRestApi csApi)
    {
        this.api = api;
        this.csApi = csApi;
    }

    [HttpGet]
    public Task<ResponseModel<PaginatedModel<CompanyGetDto>>> GetCompanies(int page = 0, int limit = 0) => api.GetAsync((HttpPagination)new(page, limit));
    [HttpGet("{companyId}")]
    public Task<ResponseModel<CompanyGetDto>> GetCompany(string companyId) => api.GetAsync(companyId);

    [HttpGet("{companyId}/sources/")]
    public Task<ResponseModel<PaginatedModel<SourceGetDto>>> GetSources(string companyId) => csApi.GetAsync(companyId);
    [HttpGet("{companyId}/sources/{sourceId:int}")]
    public Task<ResponseModel<SourceGetDto>> GetSource(string companyId, int sourceId) => csApi.GetAsync(companyId, (byte)sourceId);
    [HttpPost("{companyId}/sources/")]
    public async Task<IActionResult> PostSource(string companyId, IEnumerable<SourcePostDto> models)
    {
        var (error, sources) = await csApi.CreateUpdateDeleteAsync(companyId, models);

        return error is null ? Ok(sources) : BadRequest(error);
    }

    [HttpPost]
    public async Task<IActionResult> PostCompany(CompanyPostDto model)
    {
        var (error, _) = await api.CreateAsync(model);

        return error is null ? Ok() : BadRequest(error);
    }
    [HttpPost("collection/")]
    public async Task<IActionResult> PostCompanies(IEnumerable<CompanyPostDto> models)
    {
        var (error, _) = await api.CreateAsync(models);

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpPut("{companyId}")]
    public async Task<IActionResult> PutCompany(string companyId, CompanyPutDto model)
    {
        var (error, _) = await api.UpdateAsync(companyId, model);

        return error is null ? Ok() : BadRequest(error);
    }
    [HttpPut("collection/")]
    public async Task<IActionResult> PutCompanies(IEnumerable<CompanyPostDto> models)
    {
        var (error, _) = await api.UpdateAsync(models);

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpDelete("{companyId}")]
    public async Task<IActionResult> DeleteCompany(string companyId)
    {
        var (error, _) = await api.DeleteAsync(companyId);

        return error is null ? Ok() : BadRequest(error);
    }

    [HttpGet("sync/")]
    public async Task<string> Sync() => await api.SyncAsync();
}