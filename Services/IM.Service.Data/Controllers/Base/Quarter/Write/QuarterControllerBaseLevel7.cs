﻿using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Interfaces;
using IM.Service.Data.Services.RestApi.Common;

using Microsoft.AspNetCore.Mvc;

namespace IM.Service.Data.Controllers.Base.Quarter.Write;

public class QuarterControllerBaseLevel7<TEntity, TPost, TGet> : QuarterControllerBaseLevel6<TEntity, TPost, TGet>
    where TGet : class
    where TPost : class
    where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly RestMethodWrite<TEntity, TPost> apiWrite;
    public QuarterControllerBaseLevel7(
        RestMethodWrite<TEntity, TPost> apiWrite,
        RestMethodRead<TEntity, TGet> apiRead) : base(apiWrite, apiRead) => this.apiWrite = apiWrite;

    [HttpDelete("{companyId:string}/{sourceId:int}/{year:int}/{quarter:int}")]
    public async Task<IActionResult> Delete(string companyId, int sourceId, int year, int quarter)
    {
        var id = Activator.CreateInstance<TEntity>();
        id.CompanyId = companyId;
        id.SourceId = (byte)sourceId;
        id.Year = year;
        id.Quarter = (byte)quarter;

        var (error, _) = await apiWrite.DeleteAsync(id);

        return error is null ? Ok() : BadRequest(error);
    }
}