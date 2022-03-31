using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers.Base.Quarter.Write;

public class QuarterControllerBaseLevel6<TEntity, TPost, TGet> : QuarterControllerBaseLevel5<TEntity, TPost, TGet>
    where TGet : class
    where TPost : class
    where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly RestApiWrite<TEntity, TPost> apiWrite;
    public QuarterControllerBaseLevel6(
        RestApiWrite<TEntity, TPost> apiWrite,
        RestApiRead<TEntity, TGet> apiRead) : base(apiWrite, apiRead) => this.apiWrite = apiWrite;

    [HttpGet("load/")]
    public string Load() => apiWrite.Load();
    [HttpGet("load/{companyId}")]
    public string Load(string companyId) => apiWrite.Load(companyId);
}