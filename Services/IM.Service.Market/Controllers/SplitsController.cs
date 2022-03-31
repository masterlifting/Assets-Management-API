using IM.Service.Market.Controllers.Base.Date.Write;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers;

[ApiController, Route("[controller]")]
public class SplitsController : DateControllerBaseLevel8<Split, SplitPostDto, SplitGetDto>
{
    public SplitsController(RestApiWrite<Split, SplitPostDto> apiWrite, RestApiRead<Split, SplitGetDto> apiRead) 
        : base(apiWrite, apiRead){}
}