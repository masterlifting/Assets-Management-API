using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.DtoServices;

using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.Controllers;

[ApiController, Route("[controller]")]
public class RatingsController : ControllerBase
{
    private readonly RatingDtoManager manager;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;

    public RatingsController(
        RatingDtoManager manager,
        Repository<DataAccess.Entities.Company> companyRepository)
    {
        this.manager = manager;
        this.companyRepository = companyRepository;
    }

    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> Get(int page = 0, int limit = 0) =>
        await manager.GetAsync(new HttpPagination(page, limit));

    [HttpGet("{companyId}")]
    public async Task<ResponseModel<RatingGetDto>> Get(string companyId) => await manager.GetAsync(companyId);

    [HttpGet("{place:int}")]
    public async Task<ResponseModel<RatingGetDto>> Get(int place) => await manager.GetAsync(place);

    //[HttpGet("recalculate/")]
    //public async Task<ResponseModel<string>> Recalculate(DateTime? dateStart = null)
    //{
    //    var errors = Array.Empty<string>();

    //    foreach (var companyId in await companyRepository.GetSampleAsync(x => x.Name))
    //    {
    //        var priceResultError = await SetPriceAsync(companyId);
    //        if (priceResultError is not null)
    //            errors = errors.Append(priceResultError).ToArray();

    //        var reportResultError = await SetReportAsync(companyId);
    //        if (reportResultError is not null)
    //            errors = errors.Append(reportResultError).ToArray();
    //    }

    //    return new()
    //    {
    //        Data = "recalculating rating set",
    //        Errors = errors
    //    };
    //}
    //[HttpGet("{companyId}/recalculate/")]
    //public async Task<ResponseModel<string>> Recalculate(string companyId, DateTime? dateStart = null)
    //{
    //    var errors = Array.Empty<string>();
    //    var ctxTicker = await companyRepository.FindAsync(companyId.ToUpperInvariant().Trim());
    //    if (ctxTicker is null)
    //        return new() { Errors = new[] { $"'{companyId}' not found!" } };

    //    var priceResultError = await SetPriceAsync(companyId);
    //    if (priceResultError is not null)
    //        errors = errors.Append(priceResultError).ToArray();

    //    var reportResultError = await SetReportAsync(companyId);
    //    if (reportResultError is not null)
    //        errors = errors.Append(reportResultError).ToArray();

    //    return new()
    //    {
    //        Data = "recalculating rating set",
    //        Errors = errors
    //    };
    //}

    //private async Task<string?> SetPriceAsync(string companyId)
    //{
    //    var price = await priceRepository.FindFirstAsync(x => x.CompanyId == companyId, x => x.Date);
    //    if (price is not null)
    //        price.StatusId = (byte)StatusType.Ready;
    //    else
    //        price = new()
    //        {
    //            CompanyId = companyId,
    //            Date = new DateTime(2018, 01, 01),
    //            StatusId = (byte)StatusType.Ready,
    //        };

    //    return (await priceRepository.CreateUpdateAsync(price, $"price for '{companyId}'")).error;
    //}
    //private async Task<string?> SetReportAsync(string companyId)
    //{
    //    var report = await reportRepository.FindFirstAsync(x => x.CompanyId == companyId, x => x.Year, x => x.Quarter);
    //    if (report is not null)
    //        report.StatusId = (byte)StatusType.Ready;
    //    else
    //        report = new()
    //        {
    //            CompanyId = companyId,
    //            Year = 2018,
    //            Quarter = 1,
    //            StatusId = (byte)StatusType.Ready,
    //        };

    //    return (await reportRepository.CreateUpdateAsync(report, $"report for '{companyId}'")).error;
    //}
}