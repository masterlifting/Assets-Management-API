using System;
using System.Threading.Tasks;
using IM.Service.Recommendations.Services.Http;
using Microsoft.AspNetCore.Mvc;
using static IM.Service.Shared.Helpers.ServiceHelper;

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
            return Ok(await api.GetAsync(new Paginatior(page, limit)));
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