using IM.Service.Market.Controllers.Base.Date.Write;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class FloatsController : DateControllerBaseLevel8<Float, FloatPostDto, FloatGetDto>
{
    public FloatsController(RestApiWrite<Float, FloatPostDto> apiWrite, RestApiRead<Float, FloatGetDto> apiRead)
        : base(apiWrite, apiRead)
    {
    }
}