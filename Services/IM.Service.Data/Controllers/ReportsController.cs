using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Data.Controllers.Base.Quarter.Write;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Services.RestApi;
using IM.Service.Data.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Data.Controllers;

[ApiController, Route("[controller]")]
public class ReportsController : QuarterControllerBaseLevel7<Report, ReportPostDto, ReportGetDto>
{
    private readonly ReportApi api;
    public ReportsController(
        ReportApi api,
        RestMethodWrite<Report, ReportPostDto> apiWrite,
        RestMethodRead<Report, ReportGetDto> apiRead
        ) : base(apiWrite, apiRead) => this.api = api;

    [HttpGet("load/")]
    public string Load() => api.Load();
    [HttpGet("load/{companyId}")]
    public string Load(string companyId) => api.Load(companyId);
}