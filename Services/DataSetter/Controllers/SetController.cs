﻿using System;
using System.Collections.Generic;
using System.Linq;
using IM.Service.Common.Net.Models.Dto.Http;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using DataSetter.Clients;
using DataSetter.DataAccess;
using DataSetter.Models.Dto;
using IM.Service.Common.Net.Models.Dto.Http.Companies;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using Microsoft.EntityFrameworkCore;

using static IM.Service.Common.Net.CommonHelper;

namespace DataSetter.Controllers;

[ApiController, Route("[controller]")]
public class SetController : ControllerBase
{
    private readonly CompanyClient companyClient;
    private readonly CompanyDataClient dataClient;
    private readonly CompaniesDbContext companyContext;
    private readonly PricesDbContext pricesContext;
    private readonly ReportsDbContext reportsContext;

    public SetController(
        CompanyClient companyClient
        , CompanyDataClient dataClient
        , CompaniesDbContext companyContext
        , PricesDbContext pricesContext
        , ReportsDbContext reportsContext)
    {
        this.companyClient = companyClient;
        this.dataClient = dataClient;
        this.companyContext = companyContext;
        this.pricesContext = pricesContext;
        this.reportsContext = reportsContext;
    }

    [HttpGet("{companyId}")]
    public async Task<string> Get(string companyId)
    {
        var dbCompany = await companyContext.Companies.FindAsync(companyId.ToUpperInvariant());

        if (dbCompany is null)
            return "Company not found";

        var dbPrices = await pricesContext.Prices
            .Where(x => x.TickerName == dbCompany.Ticker)
            .OrderBy(x => x.Date)
            .ToArrayAsync();
        var dbReports = await reportsContext.Reports
            .Where(x => x.TickerName == dbCompany.Ticker)
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Quarter)
            .ToArrayAsync();

        //create models
        List<EntityTypeDto> sources = new(2);
        var prices = Array.Empty<PricePostDto>();
        var splits = Array.Empty<StockSplitPostDto>();
        var reports = Array.Empty<ReportPostDto>();
        var volumes = Array.Empty<StockVolumePostDto>();
        
        if (dbPrices.Any())
        {
            var priceSource = new EntityTypeDto
            {
                Id = dbPrices[0].SourceType == "tdameritrade" ? (byte)3 : (byte)2,
                Value = dbPrices[0].TickerNameNavigation.SourceValue
            };

            sources.Add(priceSource);

            var sourceName = string.Intern(priceSource.Id == 3 ? "tdameritrade" : "moex");

            prices = dbPrices.Select(x => new PricePostDto
            {
                CompanyId = dbCompany.Ticker,
                SourceType = sourceName,
                Date = x.Date,
                Value = x.Value
            }).ToArray();

            splits = dbCompany.StockSplits.Select(x => new StockSplitPostDto
            {
                CompanyId = dbCompany.Ticker,
                SourceType = sourceName,
                Value = x.Divider,
                Date = x.Date
            }).ToArray();
        }
        if (dbReports.Any())
        {
            var reportSource = new EntityTypeDto
            {
                Id = 4,
                Value = dbReports[0].TickerNameNavigation.SourceValue
            };

            var sourceName = string.Intern("investing");

            sources.Add(reportSource);

            reports = dbReports.Select(x => new ReportPostDto
            {
                CompanyId = dbCompany.Ticker,
                SourceType = sourceName,
                Asset = x.Asset,
                CashFlow = x.CashFlow,
                LongTermDebt = x.LongTermDebt,
                Multiplier = x.Multiplier,
                Obligation = x.Obligation,
                ProfitGross = x.ProfitGross,
                ProfitNet = x.ProfitNet,
                Quarter = x.Quarter,
                Year = x.Year,
                Revenue = x.Revenue,
                ShareCapital = x.ShareCapital,
                Turnover = x.Turnover
            }).ToArray();

            volumes = dbReports
                .GroupBy(x => x.StockVolume)
                .Select(x => new StockVolumePostDto
                {
                    CompanyId = dbCompany.Ticker,
                    SourceType = sourceName,
                    Value = x.Key,
                    Date = new DateTime
                    (
                        x.OrderBy(y => y.Year).First().Year,
                        GetLastMonth(x.OrderBy(y => y.Year).ThenBy(y => y.Quarter).First().Quarter),
                        1
                    )
                }).ToArray();
        }

        var company = new CompanyPostDto
        {
            Id = dbCompany.Ticker,
            Name = dbCompany.Name,
            Description = dbCompany.Description,
            IndustryId = (byte)dbCompany.IndustryId,
            DataSources = sources
        };

        await companyClient.Post("companies", company);
        await dataClient.Post("prices/collection", prices);
        await dataClient.Post("reports/collection", reports);
        await dataClient.Post("stockSplits/collection", splits);
        await dataClient.Post("stockVolumes/collection", volumes);

        return "Ok";
    }

    [HttpGet("collection/")]
    public async Task<ResponseModel<string>> Get()
    {
        var models = await companyContext.Companies.Select(x => new CompanyPostDto
        {
            Id = x.Ticker,
            Name = x.Name,
            Description = x.Description,
            IndustryId = (byte)x.IndustryId
        }).ToArrayAsync();

        return await companyClient.Post("companies/collection", models);
    }

    [HttpPut("{companyId}")]
    public async Task<ResponseModel<string>> Put(string companyId, CompanyPutDto model) => await companyClient.Put("companies", model, companyId);

    [HttpDelete("{companyId}")]
    public async Task<ResponseModel<string>> Delete(string companyId) => await companyClient.Delete("companies", companyId);
}