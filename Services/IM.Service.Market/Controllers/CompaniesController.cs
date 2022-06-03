using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.Http;

using Microsoft.AspNetCore.Mvc;

using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly CompanyApi api;
    private readonly CompanySourceApi csApi;

    public CompaniesController(CompanyApi api, CompanySourceApi csApi)
    {
        this.api = api;
        this.csApi = csApi;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanies(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{companyId}")]
    public async Task<IActionResult> GetCompany(string companyId)
    {
        try
        {
            return Ok(await api.GetAsync(companyId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("{companyId}/sources/")]
    public async Task<IActionResult> GetSources(string companyId)
    {
        try
        {
            return Ok(await csApi.GetAsync(companyId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("{companyId}/sources/{sourceId:int}")]
    public async Task<IActionResult> GetSource(string companyId, int sourceId)
    {
        try
        {
            return Ok(await csApi.GetAsync(companyId, (byte)sourceId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpPost("{companyId}/sources/")]
    public async Task<IActionResult> CrateSource(string companyId, IEnumerable<SourcePostDto> models)
    {
        try
        {
            return Ok(await csApi.CreateUpdateDeleteAsync(companyId, models));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("{companyId}/sources/load/")]
    public async Task<IActionResult> Load(string companyId)
    {
        try
        {
            return Ok(await GetLoaderAsync(api, companyId, null));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("{companyId}/sources/{sourceId:int}/load/")]
    public async Task<IActionResult> Load(string companyId, int sourceId)
    {
        try
        {
            return Ok(await GetLoaderAsync(api, companyId, sourceId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    
    [HttpGet("sources/load/")]
    public async Task<IActionResult> Load()
    {
        try
        {
            return Ok(await GetLoaderAsync(api, null, null));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("sources/{sourceId:int}load/")]
    public async Task<IActionResult> Load(int sourceId)
    {
        try
        {
            return Ok(await GetLoaderAsync(api, null, sourceId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompanyPostDto model)
    {
        try
        {
            return Ok(await api.CreateAsync(model));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpPost("collection/")]
    public async Task<IActionResult> PostCompanies(IEnumerable<CompanyPostDto> models)
    {
        try
        {
            return Ok(await api.CreateAsync(models));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{companyId}")]
    public async Task<IActionResult> UpdateCompany(string companyId, CompanyPutDto model)
    {
        try
        {
            return Ok(await api.UpdateAsync(companyId, model));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpPut("collection/")]
    public async Task<IActionResult> UpdateCompanies(IEnumerable<CompanyPostDto> models)
    {
        try
        {
            return Ok(await api.UpdateAsync(models));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{companyId}")]
    public async Task<IActionResult> DeleteCompany(string companyId)
    {
        try
        {
            return Ok(await api.DeleteAsync(companyId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("sync/")]
    public async Task<IActionResult> Sync()
    {
        try
        {
            return Ok(await api.SyncAsync());
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    private static Task<string> GetLoaderAsync(CompanyApi api, string? companyId, int? sourceId)
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