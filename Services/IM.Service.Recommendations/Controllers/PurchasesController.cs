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
            return Ok(await api.GetAsync(new Paginatior(page, limit), x => x.DiscountFact != null && x.DiscountPlan > 0));
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
            return Ok(await api.GetAsync(new Paginatior(page, limit), x => x.IsReady && x.DiscountPlan > 0));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("assets/")]
    public async Task<IActionResult> GetAssets()
    {
        try
        {
            return Ok(await api.GetAssetsAsync());
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("assets/{assetTypeId}")]
    public async Task<IActionResult> GetAssets(int assetTypeId, int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), x => x.AssetTypeId == assetTypeId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
    [HttpGet("assets/{assetTypeId}/ready/")]
    public async Task<IActionResult> GetAssetsReady(int assetTypeId, int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), x => x.AssetTypeId == assetTypeId && x.IsReady));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
   
    [HttpGet("assets/{assetTypeId}/{assetId}")]
    public async Task<IActionResult> Get(int assetTypeId, string assetId)
    {
        try
        {
            return Ok(await api.GetAsync((byte)assetTypeId, assetId));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}