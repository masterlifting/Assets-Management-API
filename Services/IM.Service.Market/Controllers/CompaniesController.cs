using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi;

using Microsoft.AspNetCore.Mvc;

using static IM.Service.Common.Net.Helpers.ServiceHelper;

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
}