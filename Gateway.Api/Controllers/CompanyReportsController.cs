using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using Gateway.Api.Clients;
using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;

namespace Gateway.Api.Controllers
{
    [ApiController, Route("api/companies")]
    public class ReportsController : ControllerBase
    {
        private readonly CompanyReportsClient client;
        private const string controller = "reports";
        public ReportsController(CompanyReportsClient client) => this.client = client;

        [HttpGet("reports/")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> Get(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(HttpRequestFilterType.More, year, (byte)quarter), new(page, limit));

        [HttpGet("reports/last/")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetLast(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller + "/last", GetQueryString(HttpRequestFilterType.More, year, (byte)quarter), new(page, limit));

        [HttpGet("{ticker}/reports/")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> Get(string ticker, int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(HttpRequestFilterType.More, ticker, year, (byte)quarter), new(page, limit));

        [HttpGet("{ticker}/reports/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(ticker, year), new(page, limit));
        
        [HttpGet("{ticker}/reports/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<ReportGetDto>> Get(string ticker, int year, int quarter) =>
            await client.Get<ReportGetDto>(controller, ticker, year, (byte)quarter);


        [HttpGet("reports/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(year), new(page, limit));
       
        [HttpGet("reports/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(int year, int quarter, int page = 0, int limit = 0) =>
            await client.Get<ReportGetDto>(controller, GetQueryString(year, (byte)quarter), new(page, limit));


        [HttpPost("reports/")]
        public async Task<ResponseModel<string>> Post(ReportPostDto model) => await client.Post(controller, model);
        
        [HttpPut("{ticker}/reports/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int quarter, ReportPutDto model) =>
            await client.Put(controller, model, ticker, year, quarter);

        [HttpDelete("{ticker}/reports/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<string>> Delete(string ticker, int year, int quarter) =>
            await client.Delete(controller, new ReportIdentity { TickerName = ticker, Year = year, Quarter = (byte)quarter });
    }
}
