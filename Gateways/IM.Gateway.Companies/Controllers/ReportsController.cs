using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;

using IM.Gateway.Companies.Clients;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;

namespace IM.Gateway.Companies.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportsClient client;
        private const string controller = "reports";
        public ReportsController(ReportsClient client) => this.client = client;

        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> Get(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(HttpRequestFilterType.More, year, (byte)quarter), new(page, limit));

        [HttpGet("last/")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetLast(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller + "/last", GetQueryString(HttpRequestFilterType.More, year, (byte)quarter), new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> Get(string ticker, int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(HttpRequestFilterType.More, ticker, year, (byte)quarter), new(page, limit));

        [HttpGet("{ticker}/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(ticker, year), new(page, limit));
        
        [HttpGet("{ticker}/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<ReportGetDto>> Get(string ticker, int year, int quarter) =>
            await client.Get<ReportGetDto>(controller, ticker, year, (byte)quarter);


        [HttpGet("{year:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(year), new(page, limit));
       
        [HttpGet("{year:int}/{quarter:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(int year, int quarter, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(year, (byte)quarter), new(page, limit));


        [HttpPost]
        public async Task<ResponseModel<string>> Post(ReportPostDto model) => await client.Post(controller, model);
        
        [HttpPut("{ticker}/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int quarter, ReportPutDto model) =>
            await client.Put(controller, new ReportPostDto
            {
                TickerName = ticker,
                Year = year,
                Quarter = (byte)quarter,
                SourceType = model.SourceType,
                StockVolume = model.StockVolume,
                Turnover = model.Turnover,
                LongTermDebt = model.LongTermDebt,
                Asset = model.Asset,
                CashFlow = model.CashFlow,
                Obligation = model.Obligation,
                ProfitGross = model.ProfitGross,
                ProfitNet = model.ProfitNet,
                Revenue = model.Revenue,
                ShareCapital = model.ShareCapital,
                Dividend = model.Dividend
            });
        
        [HttpDelete("{ticker}/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<string>> Delete(string ticker, int year, int quarter) =>
            await client.Delete(controller, new ReportIdentity { TickerName = ticker, Year = year, Quarter = (byte)quarter });
    }
}
