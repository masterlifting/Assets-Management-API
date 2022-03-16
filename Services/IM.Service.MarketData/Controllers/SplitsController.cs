using IM.Service.MarketData.Controllers.Base.Date.Write;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Models.Api.Http;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.MarketData.Controllers;

[ApiController, Route("[controller]")]
public class SplitsController : DateControllerBaseLevel8<Split, SplitPostDto, SplitGetDto>
{
    public SplitsController(RestMethodWrite<Split, SplitPostDto> apiWrite, RestMethodRead<Split, SplitGetDto> apiRead) 
        : base(apiWrite, apiRead){}
}