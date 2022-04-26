using DataSetter.Clients;
using DataSetter.DataAccess;

using IM.Service.Market.Models.Api.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;


namespace DataSetter.Controllers;

[ApiController, Route("[controller]")]
public class PaviamsController : ControllerBase
{
    private readonly MarketClient client;
    private readonly CompanyDataContext context;


    public PaviamsController(MarketClient client, CompanyDataContext context)
    {
        this.client = client;
        this.context = context;
    }

    [HttpPost("companies/")]
    public async Task<IActionResult> SetCompanies()
    {
        var entities = await context.Companies
            .Select(x => new
            {
                x.Id,
                x.Name,
                IndustryId = (byte)x.IndustryId,
                x.Description
            })
            .ToArrayAsync();

        return await client.Post("companies/collection", entities
            .Select(x => new CompanyPostDto
            {
                Id = x.Id,
                Name = x.Name,
                CountryId = GetCountryId(x.Id),
                IndustryId = x.IndustryId,
                Description = x.Description
            }));
    }


    [HttpPost("prices/")]
    public async Task<IActionResult> SetPrices()
    {
        var entities = await context.Prices
            .Select(x => new
            {
                x.CompanyId,
                x.SourceType,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => (x.CompanyId, x.SourceType)))
        {
            var sourceId = GetSourceId(group.Key.SourceType);

            await client.Post(GetRoute("prices", group.Key.CompanyId, sourceId, true), group
                .Select(x => new PricePostDto
                {
                    CurrencyId = GetCurrencyId(x.CompanyId),
                    Date = x.Date,
                    Value = x.Value
                }));
        }

        return Ok();
    }

    [HttpPost("reports/")]
    public async Task<IActionResult> SetReports()
    {
        var entities = await context.Reports
            .Select(x => new
            {
                x.CompanyId,
                x.Year,
                Quarter = (byte)x.Quarter,
                x.Multiplier,
                x.SourceType,
                x.Asset,
                x.CashFlow,
                x.LongTermDebt,
                x.Obligation,
                x.ProfitGross,
                x.ProfitNet,
                x.Revenue,
                x.ShareCapital,
                x.Turnover
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => (x.CompanyId, x.SourceType)))
        {
            var sourceId = GetSourceId(group.Key.SourceType);

            await client.Post(GetRoute("reports", group.Key.CompanyId, sourceId, true), group
                .Select(x => new ReportPostDto
                {
                    Year = x.Year,
                    Quarter = x.Quarter,
                    CurrencyId = GetCurrencyId(x.CompanyId),
                    Multiplier = x.Multiplier,
                    Asset = x.Asset,
                    CashFlow = x.CashFlow,
                    LongTermDebt = x.LongTermDebt,
                    Obligation = x.Obligation,
                    ProfitGross = x.ProfitGross,
                    ProfitNet = x.ProfitNet,
                    Revenue = x.Revenue,
                    ShareCapital = x.ShareCapital,
                    Turnover = x.Turnover
                }));
        }

        return Ok();
    }

    [HttpPost("splits/")]
    public async Task<IActionResult> SetSplits()
    {
        var entities = await context.StockSplits
            .Select(x => new
            {
                x.CompanyId,
                x.SourceType,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => (x.CompanyId, x.SourceType)))
        {
            var sourceId = GetSourceId(group.Key.SourceType);

            await client.Post(GetRoute("splits", group.Key.CompanyId, sourceId, true), group
            .Select(x => new SplitPostDto
            {
                Date = x.Date,
                Value = x.Value
            }));
        }

        return Ok();
    }

    [HttpPost("floats/")]
    public async Task<IActionResult> SetFloats()
    {
        var entities = await context.StockVolumes
            .Select(x => new
            {
                x.CompanyId,
                x.SourceType,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => (x.CompanyId, x.SourceType)))
        {
            var sourceId = GetSourceId(group.Key.SourceType);

            await client.Post(GetRoute("floats", group.Key.CompanyId, sourceId, true), group
            .Select(x => new FloatPostDto
            {
                Date = x.Date,
                Value = x.Value
            }));
        }

        return Ok();
    }


    [HttpPost("prices/{companyId}")]
    public async Task<IActionResult> SetPrices(string companyId)
    {
        companyId = companyId.ToUpperInvariant();
        var entities = await context.Prices
            .Where(x => x.CompanyId == companyId)
            .Select(x => new
            {
                x.SourceType,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => x.SourceType))
        {
            var sourceId = GetSourceId(group.Key);

            await client.Post(GetRoute("prices", companyId, sourceId, true), group
                .Select(x => new PricePostDto
                {
                    CurrencyId = GetCurrencyId(companyId),
                    Date = x.Date,
                    Value = x.Value
                }));
        }

        return Ok();
    }

    [HttpPost("reports/{companyId}")]
    public async Task<IActionResult> SetReports(string companyId)
    {
        var entities = await context.Reports
            .Where(x => x.CompanyId == companyId)
            .Select(x => new
            {
                x.Year,
                Quarter = (byte)x.Quarter,
                x.Multiplier,
                x.SourceType,
                x.Asset,
                x.CashFlow,
                x.LongTermDebt,
                x.Obligation,
                x.ProfitGross,
                x.ProfitNet,
                x.Revenue,
                x.ShareCapital,
                x.Turnover
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => x.SourceType))
        {
            var sourceId = GetSourceId(group.Key);

            await client.Post(GetRoute("reports", companyId, sourceId, true), group
                .Select(x => new ReportPostDto
                {
                    Year = x.Year,
                    Quarter = x.Quarter,
                    CurrencyId = GetCurrencyId(companyId),
                    Multiplier = x.Multiplier,
                    Asset = x.Asset,
                    CashFlow = x.CashFlow,
                    LongTermDebt = x.LongTermDebt,
                    Obligation = x.Obligation,
                    ProfitGross = x.ProfitGross,
                    ProfitNet = x.ProfitNet,
                    Revenue = x.Revenue,
                    ShareCapital = x.ShareCapital,
                    Turnover = x.Turnover
                }));
        }

        return Ok();
    }

    [HttpPost("splits/{companyId}")]
    public async Task<IActionResult> SetSplits(string companyId)
    {
        var entities = await context.StockSplits
            .Where(x => x.CompanyId == companyId)
            .Select(x => new
            {
                x.SourceType,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => x.SourceType))
        {
            var sourceId = GetSourceId(group.Key);

            await client.Post(GetRoute("splits", companyId, sourceId, true), group
            .Select(x => new SplitPostDto
            {
                Date = x.Date,
                Value = x.Value
            }));
        }

        return Ok();
    }

    [HttpPost("floats/{companyId}")]
    public async Task<IActionResult> SetFloats(string companyId)
    {
        var entities = await context.StockVolumes
            .Where(x => x.CompanyId == companyId)
            .Select(x => new
            {
                x.SourceType,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => x.SourceType))
        {
            var sourceId = GetSourceId(group.Key);

            await client.Post(GetRoute("floats", companyId, sourceId, true), group
            .Select(x => new FloatPostDto
            {
                Date = x.Date,
                Value = x.Value
            }));
        }

        return Ok();
    }


    [HttpPost("prices/{sourceId:int}")]
    public async Task<IActionResult> SetPrices(int sourceId)
    {
        var sourceType = GetSourceType(sourceId);
        var sId = GetSourceId(sourceType);

        var entities = await context.Prices
            .Where(x => x.SourceType == sourceType)
            .Select(x => new
            {
                x.CompanyId,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {

            await client.Post(GetRoute("prices", group.Key, sId, true), group
                .Select(x => new PricePostDto
                {
                    CurrencyId = GetCurrencyId(x.CompanyId),
                    Date = x.Date,
                    Value = x.Value
                }));
        }

        return Ok();
    }

    [HttpPost("reports/{sourceId:int}")]
    public async Task<IActionResult> SetReports(int sourceId)
    {
        var sourceType = GetSourceType(sourceId);
        var sId = GetSourceId(sourceType);

        var entities = await context.Reports
            .Where(x => x.SourceType == sourceType)
            .Select(x => new
            {
                x.CompanyId,
                x.Year,
                Quarter = (byte)x.Quarter,
                x.Multiplier,
                x.Asset,
                x.CashFlow,
                x.LongTermDebt,
                x.Obligation,
                x.ProfitGross,
                x.ProfitNet,
                x.Revenue,
                x.ShareCapital,
                x.Turnover
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            await client.Post(GetRoute("reports", group.Key, sId, true), group
                .Select(x => new ReportPostDto
                {
                    Year = x.Year,
                    Quarter = x.Quarter,
                    CurrencyId = GetCurrencyId(x.CompanyId),
                    Multiplier = x.Multiplier,
                    Asset = x.Asset,
                    CashFlow = x.CashFlow,
                    LongTermDebt = x.LongTermDebt,
                    Obligation = x.Obligation,
                    ProfitGross = x.ProfitGross,
                    ProfitNet = x.ProfitNet,
                    Revenue = x.Revenue,
                    ShareCapital = x.ShareCapital,
                    Turnover = x.Turnover
                }));
        }

        return Ok();
    }

    [HttpPost("splits/{sourceId:int}")]
    public async Task<IActionResult> SetSplits(int sourceId)
    {
        var sourceType = GetSourceType(sourceId);
        var sId = GetSourceId(sourceType);

        var entities = await context.StockSplits
            .Where(x => x.SourceType == sourceType)
            .Select(x => new
            {
                x.CompanyId,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            await client.Post(GetRoute("splits", group.Key, sId, true), group
            .Select(x => new SplitPostDto
            {
                Date = x.Date,
                Value = x.Value
            }));
        }

        return Ok();
    }

    [HttpPost("floats/{sourceId:int}")]
    public async Task<IActionResult> SetFloats(int sourceId)
    {
        var sourceType = GetSourceType(sourceId);
        var sId = GetSourceId(sourceType);

        var entities = await context.StockVolumes
            .Where(x => x.SourceType == sourceType)
            .Select(x => new
            {
                x.CompanyId,
                x.Date,
                x.Value
            })
            .ToArrayAsync();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            await client.Post(GetRoute("floats", group.Key, sId, true), group
            .Select(x => new FloatPostDto
            {
                Date = x.Date,
                Value = x.Value
            }));
        }

        return Ok();
    }


    [HttpPost("sources")]
    public async Task<IActionResult> SetSourses()
    {
        var sources = await context.CompanySources.ToArrayAsync();

        var result = new List<IActionResult>(sources.Length);

        foreach (var group in sources.GroupBy(x => x.CompanyId))
        {
            var target = group.Select(x => new SourcePostDto(GetSourceId(x.SourceId), x.Value)).ToList();
            target.Add(new SourcePostDto(1, null));

            result.Add(await client.Post($"companies/{group.Key}/sources", target));
        }

        return Ok(result.Count);
    }

    [HttpPost("sources/{companyId}")]
    public async Task<IActionResult> SetSourses(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();

        var sources = await context.CompanySources.Where(x => x.CompanyId == companyId).ToArrayAsync();

        var target = sources.Select(x => new SourcePostDto(GetSourceId(x.SourceId), x.Value)).ToList();
        target.Add(new SourcePostDto(1, null));

        return !sources.Any()
            ? BadRequest("Sources not found")
            : await client.Post($"companies/{companyId}/sources", target);
    }

    private byte GetCountryId(string companyId) => chn.Contains(companyId, StringComparer.OrdinalIgnoreCase)
        ? (byte)Countries.Chn
        : rus.Contains(companyId, StringComparer.OrdinalIgnoreCase)
            ? (byte)Countries.Rus
            : (byte)Countries.Usa;
    private byte GetCurrencyId(string companyId) => rus.Contains(companyId, StringComparer.OrdinalIgnoreCase)
            ? (byte)Currencies.Rub
            : (byte)Currencies.Usd;
    private static byte GetSourceId(string sourceType) => sourceType switch
    {
        "manual" => 1,
        "moex" => 2,
        "tdameritrade" => 4,
        "investing" => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, null)
    };
    private static byte GetSourceId(short sourceId) => sourceId switch
    {
        2 => 2,
        3 => 4,
        4 => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(sourceId), sourceId, null)
    };
    private static string GetSourceType(int sourceId) => sourceId switch
    {
        1 => "official",
        2 => "moex",
        3 => "tdameritrade",
        4 => "investing",
        _ => "manual"
    };
    private static string GetRoute(string controller, string companyId, int sourceId, bool isCollection) => isCollection
        ? $"companies/{companyId}/sources/{sourceId}/{controller}/collection"
        : $"companies/{companyId}/sources/{sourceId}/{controller}";
    private readonly string[] rus = { "AKRN", "ALRS", "CBOM", "CHMF", "CHMK", "DSKY", "ENRU", "FIVE", "GAZP", "GMKN", "HYDR", "IRKT", "ISKJ", "KAZT", "KMAZ", "LKOH", "LNTA", "LNZL", "LVHK", "MAGN", "MRKV", "MSNG", "MTLR", "MTSS", "NKHP", "NVTK", "OMZZP", "PHOR", "PLZL", "POLY", "RKKE", "ROSB", "ROSN", "RTKM", "SBER", "SELG", "SNGS", "TCSG", "TRMK", "TUZA", "UNAC", "VTBR", "YNDX", "ZILL" };
    private readonly string[] chn = { "BABA", "YY" };
}