using DataSetter.Clients;
using DataSetter.DataAccess;

using IM.Service.Market.Models.Api.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Market.Domain.Entities;
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

    [HttpGet("set/companies/")]
    public async Task<IActionResult> SetCompanies()
    {
        var companies = await context.Companies.Select(x => new CompanyPostDto()
            {
                Id = x.Id,
                Name = x.Name,
                CountryId = x.CompanySources.Any(y => y.SourceId == 2) ? (byte)Countries.Rus : (byte)Countries.Usa,
                IndustryId = (byte)x.IndustryId,
                Description = x.Description
            })
            .ToArrayAsync();

        return await client.Post("companies/collection", companies);
    }
    [HttpGet("set/sources/")]
    public async Task<IActionResult[]> SetSourses()
    {
        var sources = await context.CompanySources.ToArrayAsync();
        
        var result = new List<IActionResult>(sources.Length);
        foreach (var group in sources.GroupBy(x => x.CompanyId).Take(1))
            result.Add(await client.Post($"companies/{group.Key}/sources",
                group.Select(x => new SourcePostDto((byte) x.SourceId, x.Value))));
        
        return result.ToArray();
    }
    //[HttpGet("prices/")]
    //public async Task<ResponseModel<string>> SetPrices()
    //{
    //    List<string> errors = new();

    //    var companyIds = companyDataContext.Companies.Select(x => x.Id).ToArray();

    //    foreach (var companyId in companyIds)
    //    {
    //        var prices = companyDataContext.Prices.Where(x => x.CompanyId == companyId).Select(x => new PricePostDto
    //        {
    //            CompanyId = x.CompanyId,
    //            Date = x.Date,
    //            Value = x.Value,
    //            //SourceType = x.SourceType
    //        });

    //        var result = await companyDataClient.Post("prices/collection", prices);

    //        if (result.Errors.Any())
    //            errors.AddRange(result.Errors);
    //    }

    //    return new()
    //    {
    //        Errors = errors.ToArray()
    //    };
    //}
    //[HttpGet("reports/")]
    //public async Task<ResponseModel<string>> SetReports()
    //{
    //    List<string> errors = new();

    //    var companyIds = companyDataContext.Companies.Select(x => x.Id).ToArray();

    //    foreach (var companyId in companyIds)
    //    {
    //        var prices = companyDataContext.Reports.Where(x => x.CompanyId == companyId).Select(x => new ReportPostDto()
    //        {
    //            CompanyId = x.CompanyId,
    //            //SourceType = x.SourceType,
    //            Year = x.Year,
    //            Quarter = x.Quarter,
    //            Multiplier = x.Multiplier,
    //            Asset = x.Asset,
    //            CashFlow = x.CashFlow,
    //            LongTermDebt = x.LongTermDebt,
    //            Obligation = x.Obligation,
    //            ProfitGross = x.ProfitGross,
    //            ProfitNet = x.ProfitNet,
    //            Revenue = x.Revenue,
    //            ShareCapital = x.ShareCapital,
    //            Turnover = x.Turnover
    //        });

    //        var result = await companyDataClient.Post("reports/collection", prices);

    //        if (result.Errors.Any())
    //            errors.AddRange(result.Errors);
    //    }

    //    return new()
    //    {
    //        Errors = errors.ToArray()
    //    };
    //}
    //[HttpGet("stocksplits/")]
    //public async Task<ResponseModel<string>> SetStockSplits()
    //{
    //    var stockSplits = await companyDataContext.StockSplits.Select(x => new SplitPostDto
    //    {
    //        CompanyId = x.CompanyId,
    //        Date = x.Date,
    //        //SourceType = x.SourceType,
    //        Value = x.Value
    //    })
    //     .ToArrayAsync();

    //    return await companyDataClient.Post("stocksplits/collection", stockSplits);
    //}
    //[HttpGet("stockvolumes/")]
    //public async Task<ResponseModel<string>> SetStockVolumes()
    //{
    //    var stockVolumes = await companyDataContext.StockVolumes.Select(x => new FloatPostDto
    //        {
    //            CompanyId = x.CompanyId,
    //            Date = x.Date,
    //            //SourceType = x.SourceType,
    //            Value = x.Value
    //        })
    //        .ToArrayAsync();

    //    return await companyDataClient.Post("stockvolumes/collection", stockVolumes);
    //}
    //[HttpGet("companies_s/")]
    //public async Task<ResponseModel<string>> SetCompaniesWithSources()
    //{
    //    var companies = await companyContext.Companies.ToArrayAsync();
    //    var companySourceTypes = await companyDataContext.CompanySourceTypes.ToArrayAsync();
    //    var dtoCompanies = companies.Join(companySourceTypes.GroupBy(x => x.CompanyId),
    //        x => x.Id,
    //        y => y.Key,
    //        (x, y) => new CompanyPostDto
    //        {
    //            Id = x.Id,
    //            Name = x.Name,
    //            IndustryId = x.IndustryId,
    //            Description = x.Description,
    //            Sources = y.Select(z => new EntityTypePostDto(z.SourceTypeId,z.Value))
    //        });

    //    return await companyDataClient.Put("companies/collection", dtoCompanies);
    //}
}