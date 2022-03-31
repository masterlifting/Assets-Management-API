using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Controllers.Base.Quarter.Read;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers.Base.Quarter.Write;

public class QuarterControllerBaseLevel5<TEntity, TPost, TGet> : QuarterControllerBaseLevel4<TEntity, TGet>
    where TGet : class
    where TPost : class 
    where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly RestApiWrite<TEntity, TPost> apiWrite;
    public QuarterControllerBaseLevel5(
        RestApiWrite<TEntity, TPost> apiWrite, 
        RestApiRead<TEntity, TGet> apiRead) : base(apiRead) => this.apiWrite = apiWrite;

    [HttpPost]
    public async Task<IActionResult> Create(TPost model)
    {
        var (error, _) = await apiWrite.CreateAsync(model);

        return error is null ? Ok() : BadRequest(error);
    }
    [HttpPost("collection/")]
    public async Task<IActionResult> Create(IEnumerable<TPost> models)
    {
        var (error, _) = await apiWrite.CreateAsync(models, new DataQuarterComparer<TEntity>());

        return error is null ? Ok() : BadRequest(error);
    }
}