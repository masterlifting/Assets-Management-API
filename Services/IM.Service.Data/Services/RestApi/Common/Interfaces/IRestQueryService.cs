using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Data.Domain.Entities.Interfaces;

namespace IM.Service.Data.Services.RestApi.Common.Interfaces;

public interface IRestQueryService<TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    Task<TEntity?> GetAsync(TEntity entity);
    IQueryable<TEntity> GetQuery<T>(T filter, HttpPagination pagination) where T : class, IFilter<TEntity>;
    Task<(IQueryable<TEntity> query, int count)> GetQueryWithCountResultAsync<T>(T filter, HttpPagination pagination) where T : class, IFilter<TEntity>;
}