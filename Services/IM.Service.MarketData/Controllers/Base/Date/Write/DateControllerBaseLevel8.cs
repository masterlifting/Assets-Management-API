using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using IM.Service.MarketData.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.MarketData.Controllers.Base.Date.Write;

public class DateControllerBaseLevel8<TEntity, TPost, TGet> : DateControllerBaseLevel7<TEntity, TPost, TGet>
    where TGet : class
    where TPost : class
    where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestMethodWrite<TEntity, TPost> apiWrite;
    public DateControllerBaseLevel8(
        RestMethodWrite<TEntity, TPost> apiWrite,
        RestMethodRead<TEntity, TGet> apiRead) : base(apiWrite, apiRead) => this.apiWrite = apiWrite;

    [HttpDelete("{companyId}/{sourceId:int}/{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Delete(string companyId, int sourceId, int year, int month, int day)
    {
        var id = Activator.CreateInstance<TEntity>();
        id.CompanyId = companyId;
        id.SourceId = (byte)sourceId;
        id.Date = new DateOnly(year, month, day);

        var (error, _) = await apiWrite.DeleteAsync(id);

        return error is null ? Ok() : BadRequest(error);
    }
}