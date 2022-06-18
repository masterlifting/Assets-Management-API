using System;
using System.Threading.Tasks;
using IM.Service.Recommendations.Services.Http;
using IM.Service.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Recommendations.Controllers;

[ApiController, Route("[controller]")]
public class PurchasesController : Controller
{
    private readonly PurchaseApi api;
    public PurchasesController(PurchaseApi api) => this.api = api;

    [HttpGet]
    public async Task<IActionResult> Get(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new ServiceHelper.Paginatior(page, limit), x => x.Fact != null && x.Plan > 0));
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
            return Ok(await api.GetAsync(new ServiceHelper.Paginatior(page, limit), x => x.IsReady && x.Plan > 0));
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