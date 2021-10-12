using System;
using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Dto.Companies;
using CommonServices.Models.Http;

using Gateway.Api.Clients;
using Gateway.Api.DataAccess;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class PaviamsController : ControllerBase
    {
        private readonly DatabaseContext context;
        private readonly CompaniesClient companyClient;
        private readonly CompanyPricesClient pricesClient;
        private readonly CompanyReportsClient reportClient;
        private const string companiesController = "companies";
        private const string pricesController = "prices/collection";
        private const string reportsController = "reports/collection";
        private const string stockSplitsController = "stocksplits";
        public PaviamsController(
            DatabaseContext context
            , CompaniesClient companyClient
            , CompanyPricesClient pricesClient
            , CompanyReportsClient reportClient)
        {
            this.context = context;
            this.companyClient = companyClient;
            this.pricesClient = pricesClient;
            this.reportClient = reportClient;
        }

        [HttpPost("tickers/")]
        public async Task<ResponseModel<string>> SetTickers()
        {
            var data = await context.Companies
                .Where(x =>
                    !x.Tickers.First().Name.Equals("MFON") &&
                    !x.Tickers.First().Name.Equals("RTX"))
                .Select(x => new CompanyPostDto
                {
                    Name = x.Name,
                    Ticker = x.Tickers.First().Name,
                    Industry = x.Industry.Name,
                    Sector = x.Sector.Name,
                    IndustryId = 1,
                    PriceSourceTypeId = 1,
                    ReportSourceTypeId = 1,
                }).ToArrayAsync();

            foreach (var company in data)
                await companyClient.Post(companiesController, company);

            return new();
        }
        
        [HttpPost("prices/")]
        public async Task<ResponseModel<string>> SetPrices()
        {
            var tickers = await context.Companies
                .Where(x =>
                    !x.Tickers.First().Name.Equals("MFON") &&
                    !x.Tickers.First().Name.Equals("RTX"))
                .Select(x => new
                {
                    x.Tickers.First().Id,
                    x.Tickers.First().Name,
                    Exchange = x.Tickers.First().Exchange.Name,

                }).ToArrayAsync();

            foreach (var ticker in tickers)
            {
                var sourceType = ticker.Exchange == "СПБ" ? "tdameritrade" : "moex";
                var prices = await context.Prices
                    .Where(x => x.TickerId == ticker.Id)
                    .Select(x => new PricePostDto
                    {
                        TickerName = ticker.Name,
                        SourceType = sourceType,
                        Date = x.BidDate,
                        Value = x.Value
                    })
                    .ToArrayAsync();

                await pricesClient.Post(pricesController, prices);
            }

            return new();
        }
       
        [HttpPost("reports/")]
        public async Task<ResponseModel<string>> SetReports()
        {
            var tickers = await context.Companies
                .Where(x =>
                    !x.Tickers.First().Name.Equals("MFON") &&
                    !x.Tickers.First().Name.Equals("RTX"))
                .Select(x => new
                {
                    CompanyId = x.Id,
                    x.Tickers.First().Name
                }).ToArrayAsync();

            foreach (var ticker in tickers)
            {
                var reports = await context.Reports.Where(x => x.CompanyId == ticker.CompanyId).ToArrayAsync();

                await reportClient.Post(reportsController, reports.Select(x => new ReportPostDto
                {
                    TickerName = ticker.Name,
                    Year = x.DateReport.Year,
                    Quarter = CommonServices.CommonHelper.GetQuarter(x.DateReport.Month),
                    SourceType = "investing",
                    Multiplier = 1_000_000,
                    StockVolume = x.StockVolume,
                    Asset = x.Assets,
                    CashFlow = x.CashFlow,
                    Dividend = x.Dividends,
                    LongTermDebt = x.LongTermDebt,
                    Obligation = x.Obligations,
                    ProfitGross = x.GrossProfit,
                    ProfitNet = x.NetProfit,
                    Revenue = x.Revenue,
                    ShareCapital = x.ShareCapital,
                    Turnover = x.Turnover
                }));
            }

            return new();
        }
       
        [HttpPost("stocksplits/")]
        public async Task<ResponseModel<string>> SetStockSplits()
        {
            var tickers = await context.Companies
                .Where(x =>
                    !x.Tickers.First().Name.Equals("MFON")
                    && !x.Tickers.First().Name.Equals("RTX")
                    && x.DateSplit != null)
                .Select(x => new
                {
                    x.Tickers.First().Id,
                    x.Tickers.First().Name,
                    SplitDate = x.DateSplit
                }).ToArrayAsync();

            foreach (var ticker in tickers)
            {
                var (value, date) = ticker.Name switch
                {
                    "NEE" => (4, new DateTime(2020, 09, 28).Date),
                    "SHW" => (3, new DateTime(2021, 04, 01).Date),
                    "TSLA" => (5, new DateTime(2020, 08, 28).Date),
                    "NVDA" => (4, new DateTime(2021, 07, 20).Date),
                    "AAPL" => (4, new DateTime(2020, 08, 28).Date),
                    _ => throw new ArgumentOutOfRangeException()
                };

                await companyClient.Post(stockSplitsController, new StockSplitPostDto
                {
                    TickerName = ticker.Name,
                    Value = value,
                    Date = date
                });
            }

            return new();
        }
       
        [HttpPost("loadprices/")]
        public async Task<ResponseModel<string>> LoadPrices()
        {
            var tickers = await context.Companies
                .Where(x =>
                    !x.Tickers.First().Name.Equals("MFON") &&
                    !x.Tickers.First().Name.Equals("RTX"))
                .Select(x => new
                {
                    Company = x.Name,
                    x.Tickers.First().Name,
                    SourceValue = x.ReportSource.Value,
                    Exchange = x.Tickers.First().Exchange.Name
                }).ToArrayAsync();


            foreach (var ticker in tickers)
            {
                var sourceTypeId = ticker.Exchange == "СПБ" ? (byte)3 : (byte)2;

                await companyClient.Put(companiesController, new CompanyPutDto
                {
                    Name = ticker.Company,
                    PriceSourceTypeId = sourceTypeId,
                    ReportSourceTypeId = 1,
                    IndustryId = 0
                }, ticker.Name);

                await Task.Delay(1000);
            }

            return new();
        }
        
        [HttpPost("loadreports/")]
        public async Task<ResponseModel<string>> LoadReports()
        {
            var tickers = await context.Companies
                .Where(x =>
                    !x.Tickers.First().Name.Equals("MFON") &&
                    !x.Tickers.First().Name.Equals("RTX"))
                .Select(x => new
                {
                    Company = x.Name,
                    x.Tickers.First().Name,
                    SourceValue = x.ReportSource.Value,
                    Exchange = x.Tickers.First().Exchange.Name
                }).ToArrayAsync();


            foreach (var ticker in tickers)
            {
                var sourceTypeId = ticker.Exchange == "СПБ" ? (byte)3 : (byte)2;

                await companyClient.Put(companiesController, new CompanyPutDto
                {
                    Name = ticker.Company,
                    PriceSourceTypeId = sourceTypeId,
                    ReportSourceTypeId = 3,
                    ReportSourceValue = ticker.SourceValue,
                    IndustryId = 0
                }, ticker.Name);

                await Task.Delay(10000);
            }

            return new();
        }
    }
}
