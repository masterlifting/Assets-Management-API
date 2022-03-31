using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Services.RestApi.Common.Interfaces;

public interface IRestQueryService<TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    IQueryable<TEntity> GetQuery<T>(T filter, HttpPagination pagination) where T : class, IFilter<TEntity>;

    Task<TEntity?> GetAsync(TEntity entity);
    Task<(IQueryable<TEntity> query, int count)> GetQueryWithCountResultAsync<T>(T filter, HttpPagination pagination) where T : class, IFilter<TEntity>;
}