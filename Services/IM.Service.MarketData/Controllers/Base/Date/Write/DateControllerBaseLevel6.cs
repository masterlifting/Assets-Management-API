using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.MarketData.Controllers.Base.Date.Write;

public class DateControllerBaseLevel6<TEntity, TPost, TGet> : DateControllerBaseLevel5<TEntity, TPost, TGet>
    where TGet : class
    where TPost : class 
    where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestMethodWrite<TEntity, TPost> apiWrite;
    public DateControllerBaseLevel6(
        RestMethodWrite<TEntity, TPost> apiWrite, 
        RestMethodRead<TEntity, TGet> apiRead) : base(apiWrite, apiRead) => this.apiWrite = apiWrite;

    [HttpGet("load/")]
    public string Load() => apiWrite.Load();
    [HttpGet("load/{companyId}")]
    public string Load(string companyId) => apiWrite.Load(companyId);
}