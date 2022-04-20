using IM.Service.Market.Controllers.Base;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers;

[ApiController]
[Route("companies/[controller]")]
[Route("companies/{companyId}/[controller]")]
[Route("companies/{companyId}/sources/{sourceId:int}/[controller]")]
public class FloatsController : DateControllerBase<Float, FloatPostDto, FloatGetDto>
{
    public FloatsController(RestApiWrite<Float, FloatPostDto> apiWrite, RestApiRead<Float, FloatGetDto> apiRead)
        : base(apiWrite, apiRead)
    {
    }
}