using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.RestApi;

using Microsoft.AspNetCore.Mvc;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helpers.ServiceHelper;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class RatingController : ControllerBase
{
    private readonly RatingRestApi api;
    public RatingController(RatingRestApi api) => this.api = api;

    [HttpGet]
    public async Task<IActionResult> Get(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), nameof(Rating)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("{companyId}")]
    public async Task<IActionResult> GetByCompany(string companyId)
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

    [HttpGet("{place:int}")]
    public async Task<IActionResult> GetByPlace(int place)
    {
        try
        {
            return Ok(await api.GetAsync(place));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("price/")]
    public async Task<IActionResult> GetPriceResultOrdered(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), nameof(Price)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("report/")]
    public async Task<IActionResult> GetReportResultOrdered(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), nameof(Report)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("coefficient/")]
    public async Task<IActionResult> GetCoefficientResultOrdered(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), nameof(Coefficient)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("dividend/")]
    public async Task<IActionResult> GetDividendResultOrdered(int page = 0, int limit = 0)
    {
        try
        {
            return Ok(await api.GetAsync(new Paginatior(page, limit), nameof(Dividend)));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }


    [HttpGet("recalculate/")]
    public async Task<IActionResult> Recalculate()
    {
        try
        {
            return Ok(await api.RecalculateAsync(CompareType.Equal, null, 2016));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("recalculate/{companyId}")]
    public async Task<IActionResult> Recalculate(string companyId)
    {
        try
        {
            return Ok(await api.RecalculateAsync(CompareType.Equal, companyId, 2016));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("recalculate/{companyId}/{year:int}")]
    public async Task<IActionResult> Recalculate(string companyId, int year)
    {
        try
        {
            return Ok(await api.RecalculateAsync(CompareType.Equal, companyId, year));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("recalculate/{companyId}/{year:int}/{month:int}")]
    public async Task<IActionResult> Recalculate(string companyId, int year, int month)
    {
        try
        {
            return Ok(await api.RecalculateAsync(CompareType.Equal, companyId, year, month));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("recalculate/{companyId}/{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Recalculate(string companyId, int year, int month, int day)
    {
        try
        {
            return Ok(await api.RecalculateAsync(CompareType.Equal, companyId, year, month, day));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("recalculate/{year:int}")]
    public async Task<IActionResult> Recalculate(int year)
    {
        try
        {
            return Ok(await api.RecalculateAsync(CompareType.Equal, null, year));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("recalculate/{year:int}/{month:int}")]
    public async Task<IActionResult> Recalculate(int year, int month)
    {
        try
        {
            return Ok(await api.RecalculateAsync(CompareType.Equal, null, year, month));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("recalculate/{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Recalculate(int year, int month, int day)
    {
        try
        {
            return Ok(await api.RecalculateAsync(CompareType.Equal, null, year, month, day));
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}