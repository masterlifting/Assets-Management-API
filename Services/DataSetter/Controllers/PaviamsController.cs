using DataSetter.Clients;
using DataSetter.DataAccess.Company;
using DataSetter.DataAccess.CompanyData;

using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Market.Models.Api.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CompanyPostDto = DataSetter.Models.Dto.CompanyPostDto;

namespace DataSetter.Controllers;

[ApiController, Route("[controller]")]
public class PaviamsController : ControllerBase
{
    private readonly CompanyDataClient companyDataClient;
    private readonly CompanyDatabaseContext companyContext;
    private readonly CompanyDataDatabaseContext companyDataContext;


    public PaviamsController(
        CompanyDataClient companyDataClient
        , CompanyDatabaseContext companyContext
        , CompanyDataDatabaseContext companyDataContext)
    {
        this.companyDataClient = companyDataClient;
        this.companyContext = companyContext;
        this.companyDataContext = companyDataContext;
    }

    [HttpGet("companies/")]
    public async Task<ResponseModel<string>> SetCompanies()
    {
        var companies = await companyContext.Companies.Select(x => new CompanyPostDto
        {
            Id = x.Id,
            Name = x.Name,
            IndustryId = x.IndustryId,
            Description = x.Description
        })
        .ToArrayAsync();

        return await companyDataClient.Post("companies/collection", companies);
    }
    [HttpGet("prices/")]
    public async Task<ResponseModel<string>> SetPrices()
    {
        List<string> errors = new();

        var companyIds = companyDataContext.Companies.Select(x => x.Id).ToArray();

        foreach (var companyId in companyIds)
        {
            var prices = companyDataContext.Prices.Where(x => x.CompanyId == companyId).Select(x => new PricePostDto
            {
                CompanyId = x.CompanyId,
                Date = x.Date,
                Value = x.Value,
                //SourceType = x.SourceType
            });

            var result = await companyDataClient.Post("prices/collection", prices);

            if (result.Errors.Any())
                errors.AddRange(result.Errors);
        }

        return new()
        {
            Errors = errors.ToArray()
        };
    }
    [HttpGet("reports/")]
    public async Task<ResponseModel<string>> SetReports()
    {
        List<string> errors = new();

        var companyIds = companyDataContext.Companies.Select(x => x.Id).ToArray();

        foreach (var companyId in companyIds)
        {
            var prices = companyDataContext.Reports.Where(x => x.CompanyId == companyId).Select(x => new ReportPostDto()
            {
                CompanyId = x.CompanyId,
                //SourceType = x.SourceType,
                Year = x.Year,
                Quarter = x.Quarter,
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
            });

            var result = await companyDataClient.Post("reports/collection", prices);

            if (result.Errors.Any())
                errors.AddRange(result.Errors);
        }

        return new()
        {
            Errors = errors.ToArray()
        };
    }
    [HttpGet("stocksplits/")]
    public async Task<ResponseModel<string>> SetStockSplits()
    {
        var stockSplits = await companyDataContext.StockSplits.Select(x => new SplitPostDto
        {
            CompanyId = x.CompanyId,
            Date = x.Date,
            //SourceType = x.SourceType,
            Value = x.Value
        })
         .ToArrayAsync();

        return await companyDataClient.Post("stocksplits/collection", stockSplits);
    }
    [HttpGet("stockvolumes/")]
    public async Task<ResponseModel<string>> SetStockVolumes()
    {
        var stockVolumes = await companyDataContext.StockVolumes.Select(x => new FloatPostDto
            {
                CompanyId = x.CompanyId,
                Date = x.Date,
                //SourceType = x.SourceType,
                Value = x.Value
            })
            .ToArrayAsync();

        return await companyDataClient.Post("stockvolumes/collection", stockVolumes);
    }
    [HttpGet("companies_s/")]
    public async Task<ResponseModel<string>> SetCompaniesWithSources()
    {
        var companies = await companyContext.Companies.ToArrayAsync();
        var companySourceTypes = await companyDataContext.CompanySourceTypes.ToArrayAsync();
        var dtoCompanies = companies.Join(companySourceTypes.GroupBy(x => x.CompanyId),
            x => x.Id,
            y => y.Key,
            (x, y) => new CompanyPostDto
            {
                Id = x.Id,
                Name = x.Name,
                IndustryId = x.IndustryId,
                Description = x.Description,
                Sources = y.Select(z => new EntityTypePostDto(z.SourceTypeId,z.Value))
            });

        return await companyDataClient.Put("companies/collection", dtoCompanies);
    }
}