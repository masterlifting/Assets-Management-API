using System;
using System.Threading.Tasks;
using IM.Service.Recommendations.Services.Http;

using Microsoft.AspNetCore.Mvc;
using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Recommendations.Controllers;

[ApiController, Route("[controller]")]
public class SalesController : Controller
{
    private readonly SaleApi api;
    public SalesController(SaleApi api) => this.api = api;

    [HttpGet]
    public async Task<IActionResult> Get(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), x => x.Fact != null && x.Plan > 0));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("ready/")]
    public async Task<IActionResult> GetReady(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), x => x.IsReady && x.Plan > 0));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("payback/")]
    public async Task<IActionResult> GetPayback(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), x => x.Fact != null && x.Plan == 0));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{companyId}")]
    public async Task<IActionResult> Get(string companyId)
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
}