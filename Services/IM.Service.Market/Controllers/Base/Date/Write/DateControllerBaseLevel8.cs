using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers.Base.Date.Write;

public class DateControllerBaseLevel8<TEntity, TPost, TGet> : DateControllerBaseLevel7<TEntity, TPost, TGet>
    where TGet : class
    where TPost : class
    where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly RestApiWrite<TEntity, TPost> apiWrite;
    public DateControllerBaseLevel8(
        RestApiWrite<TEntity, TPost> apiWrite,
        RestApiRead<TEntity, TGet> apiRead) : base(apiWrite, apiRead) => this.apiWrite = apiWrite;

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