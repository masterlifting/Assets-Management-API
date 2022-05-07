using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Services.HttpRestApi.Common.Interfaces;

public interface IRestQueryService<TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    IQueryable<TEntity> GetQuery<T>(T filter) where T : class, IFilter<TEntity>;
    Task<(IQueryable<TEntity> query, int count)> GetQueryWithCountAsync<T>(T filter, ServiceHelper.Paginatior pagination) where T : class, IFilter<TEntity>;
}