using CommonServices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;

using IM.Service.Company.Reports.Services.DtoServices;
using IM.Service.Company.Reports.Services.ReportServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Service.Company.Reports.Controllers
{
    [ApiController, Route("[controller]")]
    public class ReportsController : Controller
    {
        private readonly DtoManager manager;
        private readonly ReportLoader loader;
        public ReportsController(DtoManager manager, ReportLoader loader)
        {
            this.manager = manager;
            this.loader = loader;
        }

        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> Get(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(CommonEnums.HttpRequestFilterType.More, year, quarter), new(page, limit));

        [HttpGet("last/")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetLast(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await manager.GetLastAsync(new(CommonEnums.HttpRequestFilterType.More, year, quarter), new(page, limit));

        [HttpGet("{ticker}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> Get(string ticker, int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(CommonEnums.HttpRequestFilterType.More, ticker, year, quarter), new(page, limit));

        [HttpGet("{ticker}/{year:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(string ticker, int year, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(ticker, year), new(page, limit));

        [HttpGet("{ticker}/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<ReportGetDto>> Get(string ticker, int year, int quarter) =>
            await manager.GetAsync(new ReportIdentity() { TickerName = ticker, Year = year, Quarter = (byte)quarter });


        [HttpGet("{year:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(year), new(page, limit));

        [HttpGet("{year:int}/{quarter:int}")]
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(int year, int quarter, int page = 0, int limit = 0) =>
            await manager.GetAsync(new(year, quarter), new(page, limit));


        [HttpPost]
        public async Task<ResponseModel<string>> Post(ReportPostDto model) => await manager.CreateAsync(model);

        [HttpPut("{ticker}/{year:int}/{quarter:int}")]
        public async Task<ResponseModel<string>> Put(string ticker, int year, int quarter, ReportPutDto model) =>
            await manager.UpdateAsync(new ReportPostDto
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
            await manager.DeleteAsync(new ReportIdentity { TickerName = ticker, Year = year, Quarter = (byte)quarter });

        [HttpPost("load/")]
        public async Task<string> Load()
        {
            var loadedCount = await loader.LoadAsync();
            return $"loaded reports count: {loadedCount}";
        }
    }
}
