using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.MarketData.Controllers.Base.Quarter.Write;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.MarketData.Controllers;

[ApiController, Route("[controller]")]
public class ReportsController : QuarterControllerBaseLevel8<Report, ReportPostDto, ReportGetDto>
{
    public ReportsController( RestMethodWrite<Report, ReportPostDto> apiWrite, RestMethodRead<Report, ReportGetDto> apiRead) 
        : base(apiWrite, apiRead){}
}