using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Services.DataServices.Reports;
using IM.Service.Company.Data.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.CommonEnums;

namespace IM.Service.Company.Data.Controllers;

[ApiController, Route("[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportsDtoManager manager;
    private readonly ReportLoader loader;
    public ReportsController(ReportsDtoManager manager, ReportLoader loader)
    {
        this.manager = manager;
        this.loader = loader;
    }

    public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> Get(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByQuarter<Report>(HttpRequestFilterType.More, year, quarter), new(page, limit));

    [HttpGet("last/")]
    public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetLast(int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
        await manager.GetLastAsync(new CompanyDataFilterByQuarter<Report>(HttpRequestFilterType.More, year, quarter), new(page, limit));

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> Get(string companyId, int year = 0, int quarter = 0, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByQuarter<Report>(HttpRequestFilterType.More, companyId, year, quarter), new(page, limit));

    [HttpGet("{companyId}/{Year:int}")]
    public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(string companyId, int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByQuarter<Report>(companyId, year), new(page, limit));

    [HttpGet("{companyId}/{Year:int}/{Quarter:int}")]
    public async Task<ResponseModel<ReportGetDto>> Get(string companyId, int year, int quarter) =>
        await manager.GetAsync(companyId, year, (byte)quarter);


    [HttpGet("{Year:int}")]
    public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(int year, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByQuarter<Report>(year), new(page, limit));

    [HttpGet("{Year:int}/{Quarter:int}")]
    public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetEqual(int year, int quarter, int page = 0, int limit = 0) =>
        await manager.GetAsync(new CompanyDataFilterByQuarter<Report>(year, quarter), new(page, limit));


    [HttpPost]
    public async Task<ResponseModel<string>> Post(ReportPostDto model) => await manager.CreateAsync(model);
    [HttpPost("collection/")]
    public async Task<ResponseModel<string>> Post(IEnumerable<ReportPostDto> models) => await manager.CreateAsync(models);

    [HttpPut("{companyId}/{Year:int}/{Quarter:int}")]
    public async Task<ResponseModel<string>> Put(string companyId, int year, int quarter, ReportPutDto model) =>
        await manager.UpdateAsync(new ReportPostDto
        {
            CompanyId = companyId,
            Year = year,
            Quarter = (byte)quarter,
            SourceType = model.SourceType,
            Multiplier = model.Multiplier,
            Turnover = model.Turnover,
            LongTermDebt = model.LongTermDebt,
            Asset = model.Asset,
            CashFlow = model.CashFlow,
            Obligation = model.Obligation,
            ProfitGross = model.ProfitGross,
            ProfitNet = model.ProfitNet,
            Revenue = model.Revenue,
            ShareCapital = model.ShareCapital,
        });

    [HttpDelete("{companyId}/{Year:int}/{Quarter:int}")]
    public async Task<ResponseModel<string>> Delete(string companyId, int year, int quarter) =>
        await manager.DeleteAsync(companyId, year, (byte)quarter);

    [HttpPost("load/")]
    public async Task<string> Load()
    {
        var reports = await loader.DataSetAsync();
        return $"Loaded reports count: {reports.GroupBy(x => x.CompanyId).Count()} is loaded";
    }
}