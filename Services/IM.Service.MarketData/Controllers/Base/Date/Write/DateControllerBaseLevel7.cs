using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.MarketData.Controllers.Base.Date.Write;

public class DateControllerBaseLevel7<TEntity, TPost, TGet> : DateControllerBaseLevel6<TEntity, TPost, TGet>
    where TGet : class
    where TPost : class
    where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestMethodWrite<TEntity, TPost> apiWrite;
    public DateControllerBaseLevel7(
        RestMethodWrite<TEntity, TPost> apiWrite,
        RestMethodRead<TEntity, TGet> apiRead) : base(apiWrite, apiRead) => this.apiWrite = apiWrite;

    [HttpPut("{companyId}/{sourceId:int}/{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Update(string companyId, int sourceId, int year, int month, int day, TPost model)
    {
        var id = Activator.CreateInstance<TEntity>();
        id.CompanyId = companyId;
        id.SourceId = (byte)sourceId;
        id.Date = new DateOnly(year, month, day);

        var (error, _) = await apiWrite.UpdateAsync(id, model);

        return error is null ? Ok() : BadRequest(error);
    }
}