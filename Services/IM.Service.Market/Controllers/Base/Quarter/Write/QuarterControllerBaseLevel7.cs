using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common;
using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Market.Controllers.Base.Quarter.Write;

public class QuarterControllerBaseLevel7<TEntity, TPost, TGet> : QuarterControllerBaseLevel6<TEntity, TPost, TGet>
    where TGet : class
    where TPost : class
    where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly RestApiWrite<TEntity, TPost> apiWrite;
    public QuarterControllerBaseLevel7(
        RestApiWrite<TEntity, TPost> apiWrite,
        RestApiRead<TEntity, TGet> apiRead) : base(apiWrite, apiRead) => this.apiWrite = apiWrite;

    [HttpPut("{companyId}/{sourceId:int}/{year:int}/{quarter:int}")]
    public async Task<IActionResult> Update(string companyId, int sourceId, int year, int quarter, TPost model)
    {
        var id = Activator.CreateInstance<TEntity>();
        id.CompanyId = companyId;
        id.SourceId = (byte)sourceId;
        id.Year = year;
        id.Quarter = (byte)quarter;

        var (error, _) = await apiWrite.UpdateAsync(id, model);

        return error is null ? Ok() : BadRequest(error);
    }
}