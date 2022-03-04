using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Controllers.Base.Quarter.Read;
using IM.Service.Data.Domain.DataAccess.Comparators;
using IM.Service.Data.Domain.Entities.Interfaces;
using IM.Service.Data.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Data.Controllers.Base.Quarter.Write;

public class QuarterControllerBaseLevel5<TEntity, TPost, TGet> : QuarterControllerBaseLevel4<TEntity, TGet>
    where TGet : class
    where TPost : class 
    where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly RestMethodWrite<TEntity, TPost> apiWrite;
    public QuarterControllerBaseLevel5(
        RestMethodWrite<TEntity, TPost> apiWrite, 
        RestMethodRead<TEntity, TGet> apiRead) : base(apiRead) => this.apiWrite = apiWrite;

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