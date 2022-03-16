using IM.Service.MarketData.Controllers.Base.Date.Write;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Models.Api.Http;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.MarketData.Controllers;

[ApiController, Route("[controller]")]
public class PricesController : DateControllerBaseLevel8<Price, PricePostDto, PricePostDto>
{
    public PricesController(RestMethodWrite<Price, PricePostDto> apiWrite, RestMethodRead<Price, PricePostDto> apiRead)
        : base(apiWrite, apiRead)
    {
    }
}