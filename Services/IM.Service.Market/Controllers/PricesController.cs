using IM.Service.Market.Controllers.Base.Date.Write;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class PricesController : DateControllerBaseLevel8<Price, PricePostDto, PriceGetDto>
{
    public PricesController(RestApiWrite<Price, PricePostDto> apiWrite, RestApiRead<Price, PriceGetDto> apiRead)
        : base(apiWrite, apiRead)
    {
    }
}