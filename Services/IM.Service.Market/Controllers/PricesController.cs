using IM.Service.Market.Controllers.Base;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.HttpRestApi.Common;

using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers;

[ApiController]
[Route("companies/[controller]")]
[Route("companies/{companyId}/[controller]")]
[Route("companies/{companyId}/sources/{sourceId:int}/[controller]")]
public class PricesController : DateControllerBase<Price, PricePostDto, PriceGetDto>
{
    public PricesController(RestApiWrite<Price, PricePostDto> apiWrite, RestApiRead<Price, PriceGetDto> apiRead) : base(apiWrite, apiRead) { }
}