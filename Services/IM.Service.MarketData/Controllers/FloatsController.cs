using IM.Service.MarketData.Controllers.Base.Date.Write;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Models.Api.Http;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.MarketData.Controllers;

[ApiController, Route("[controller]")]
public class FloatsController : DateControllerBaseLevel8<Float, FloatPostDto, FloatGetDto>
{
    public FloatsController(RestMethodWrite<Float, FloatPostDto> apiWrite, RestMethodRead<Float, FloatGetDto> apiRead)
        : base(apiWrite, apiRead)
    {
    }
}