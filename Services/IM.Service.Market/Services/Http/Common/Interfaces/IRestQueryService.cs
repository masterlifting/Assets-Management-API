using IM.Service.Shared.Helpers;
using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Shared.SqlAccess.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Services.Http.Common.Interfaces;

public interface IRestQueryService<TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    IQueryable<TEntity> GetQuery<T>(T filter) where T : class, IFilter<TEntity>;
    Task<(IQueryable<TEntity> query, int count)> GetQueryWithCountAsync<T>(T filter, ServiceHelper.Paginatior pagination) where T : class, IFilter<TEntity>;
}