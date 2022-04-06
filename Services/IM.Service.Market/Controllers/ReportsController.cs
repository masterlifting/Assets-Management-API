using IM.Service.Market.Controllers.Base.Quarter.Write;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class ReportsController : QuarterControllerBaseLevel8<Report, ReportPostDto, ReportGetDto>
{
    public ReportsController( RestApiWrite<Report, ReportPostDto> apiWrite, RestApiRead<Report, ReportGetDto> apiRead) 
        : base(apiWrite, apiRead)
    {
    }
}