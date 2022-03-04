using IM.Service.Data.Controllers.Base.Date.Write;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Models.Api.Http;
using IM.Service.Data.Services.RestApi;
using IM.Service.Data.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Data.Controllers;

[ApiController, Route("[controller]")]
public class PricesController : DateControllerBaseLevel7<Price, PricePostDto, PricePostDto>
{
    private readonly PriceApi api;
    public PricesController(
        PriceApi api,
        RestMethodWrite<Price, PricePostDto> apiWrite,
        RestMethodRead<Price, PricePostDto> apiRead
        ) : base(apiWrite, apiRead) => this.api = api;




    [HttpGet("load/")]
    public string Load() => api.Load();
    [HttpGet("load/{companyId}")]
    public string Load(string companyId) => api.Load(companyId);
}