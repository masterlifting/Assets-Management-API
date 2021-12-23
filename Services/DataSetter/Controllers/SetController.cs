using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using DataSetter.Clients;
using DataSetter.DataAccess;
using DataSetter.Models.Dto;

using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using static IM.Service.Common.Net.CommonHelper.QarterHelper;

namespace DataSetter.Controllers;

[ApiController, Route("[controller]")]
public class SetController : ControllerBase
{
    private readonly CompanyClient companyClient;
    private readonly CompanyDataClient dataClient;
    private readonly InvestmentManagerContext context;


    public SetController(
        CompanyClient companyClient
        , CompanyDataClient dataClient
        , InvestmentManagerContext context)
    {
        this.companyClient = companyClient;
        this.dataClient = dataClient;
        this.context = context;
    }

    //[HttpGet("{companyId}")]
    public async Task<string> Get(long tickerId)
    {
        var ticker = await context.Tickers.FindAsync(tickerId);

        if (ticker is null)
            return "ticker not found";

        var dbCompany = await context.Companies.FindAsync(ticker.CompanyId);

        if (dbCompany is null)
            return "Company not found";

        //var dbPrices = await context.Prices
        //    .Where(x => x.TickerId == tickerId)
        //    .OrderBy(x => x.BidDate)
        //    .ToArrayAsync();
        //var dbReports = await context.Reports
        //    .Where(x => x.CompanyId == ticker.CompanyId)
        //    .OrderBy(x => x.DateReport)
        //    .ToArrayAsync();

        //create models
        StockSplitPostDto? split = null;
            if (dbCompany.DateSplit is not null)
                split = new StockSplitPostDto
                {
                    CompanyId = ticker.Name,
                    SourceType = "manual",
                    Value = 1,
                    Date = dbCompany.DateSplit.Value
                };
        //List<EntityTypeDto> sources = new(2);
        //var prices = Array.Empty<PricePostDto>();
        //var reports = Array.Empty<ReportPostDto>();
        //var volumes = Array.Empty<StockVolumePostDto>();

        //if (dbPrices.Any())
        //{
        //    var exchange = await context.Exchanges.FindAsync(ticker.ExchangeId);

        //    var priceSource = new EntityTypeDto
        //    {
        //        Id = exchange!.Id == 2 ? (byte)3 : (byte)2,
        //        Value = ticker.Name
        //    };

        //    sources.Add(priceSource);

        //    var sourceName = string.Intern(priceSource.Id == 3 ? "tdameritrade" : "moex");

        //    prices = dbPrices.Select(x => new PricePostDto
        //    {
        //        CompanyId = ticker.Name,
        //        SourceType = sourceName,
        //        Date = x.BidDate,
        //        Value = x.Value
        //    }).ToArray();

        //}
        //if (dbReports.Any())
        //{
        //    var source = await context.ReportSources.FirstOrDefaultAsync(x => x.CompanyId == ticker.CompanyId);
        //    var reportSource = new EntityTypeDto
        //    {
        //        Id = 4,
        //        Value = source!.Value
        //    };

        //    var sourceName = string.Intern("investing");

        //    sources.Add(reportSource);

        //    reports = dbReports.Select(x => new ReportPostDto
        //    {
        //        CompanyId = ticker.Name,
        //        SourceType = sourceName,
        //        Asset = x.Assets,
        //        CashFlow = x.CashFlow,
        //        LongTermDebt = x.LongTermDebt,
        //        Multiplier = 1_000_000,
        //        Obligation = x.Obligations,
        //        ProfitGross = x.GrossProfit,
        //        ProfitNet = x.NetProfit,
        //        Quarter = GetQuarter(x.DateReport.Month),
        //        Year = x.DateReport.Year,
        //        Revenue = x.Revenue,
        //        ShareCapital = x.ShareCapital,
        //        Turnover = x.Turnover
        //    }).ToArray();

        //    volumes = dbReports
        //        .GroupBy(x => x.StockVolume)
        //        .Select(x => new StockVolumePostDto
        //        {
        //            CompanyId = ticker.Name,
        //            SourceType = sourceName,
        //            Value = x.Key,
        //            Date = x.OrderBy(y => y.DateReport).First().DateReport
        //        }).ToArray();
        //}

        //var company = new CompanyPostDto
        //{
        //    Id = ticker.Name,
        //    Name = dbCompany.Name,
        //    Description = "",
        //    IndustryId = (byte)dbCompany.IndustryId,
        //    DataSources = sources
        //};

        //await companyClient.Put("companies", company, company.Id);
        //await dataClient.Post("prices/collection", prices);
        //await dataClient.Post("reports/collection", reports);
        if (split is not null)
            await dataClient.Post("stockSplits", split);
        //await dataClient.Post("stockVolumes/collection", volumes);

        return ticker.Name;
    }
    [HttpGet]
    public async Task<ResponseModel<string>> Get()
    {
        var tickers = await context.Tickers.Where(x => x.Prices.Any(y => y.BidDate >= DateTime.Now.AddMonths(-3))).ToArrayAsync();
        var data = tickers.Select(x => x.Id).ToImmutableArray();

        var result = new string[data.Length];

        for (var i = 0; i < result.Length; i++)
        {
            var r = await Get(data[i]);
            result[i] = $"{data[i]}\t-> {r}";
            //await Task.Delay(3000);
        }
        return new() { Errors = result };
    }

    //[HttpPut("{companyId}")]
    //public async Task<string> Put(string companyId)
    //{
    //    companyId = companyId.ToUpperInvariant();

    //    var dbCompany = await context.Companies.FindAsync(companyId);

    //    if (dbCompany is null)
    //        return "Company not found";

    //    var dbPrices = await context.Prices
    //        .Where(x => x.TickerName == dbCompany.Ticker)
    //        .OrderBy(x => x.Date)
    //        .ToArrayAsync();
    //    var dbReports = await context.Reports
    //        .Where(x => x.TickerName == dbCompany.Ticker)
    //        .OrderBy(x => x.Year)
    //        .ThenBy(x => x.Quarter)
    //        .ToArrayAsync();

    //    List<EntityTypeDto> sources = new(2);
    //    if (dbPrices.Any())
    //    {
    //        var priceSource = new EntityTypeDto
    //        {
    //            Id = dbPrices[0].SourceType == "tdameritrade" ? (byte)3 : (byte)2,
    //            Value = dbPrices[0].TickerNameNavigation.SourceValue
    //        };
    //        sources.Add(priceSource);
    //    }
    //    if (dbReports.Any())
    //    {
    //        var reportSource = new EntityTypeDto
    //        {
    //            Id = 4,
    //            Value = dbReports[0].TickerNameNavigation.SourceValue
    //        };
    //        sources.Add(reportSource);
    //    }

    //    var company = new CompanyPutDto
    //    {
    //        Name = dbCompany.Name,
    //        Description = dbCompany.Description,
    //        IndustryId = (byte)dbCompany.IndustryId,
    //        DataSources = sources
    //    };

    //    var response = await companyClient.Put("companies", company, companyId);

    //    return response.Data ?? string.Join(';', response.Errors);
    //}

    //[HttpPut]
    //public async Task<ResponseModel<string>> Put()
    //{
    //    var companyIds = await context.Companies.Select(x => x.Ticker).ToArrayAsync();
    //    var result = new string[companyIds.Length];

    //    for (var i = 0; i < result.Length; i++)
    //    {
    //        var r = await Put(companyIds[i]);
    //        result[i] = $"{companyIds[i]} -> {r}";
    //    }
    //    return new() { Errors = result };
    //}

    //[HttpDelete("{companyId}")]
    //public async Task<ResponseModel<string>> Delete(string companyId) => await companyClient.Delete("companies", companyId);
}