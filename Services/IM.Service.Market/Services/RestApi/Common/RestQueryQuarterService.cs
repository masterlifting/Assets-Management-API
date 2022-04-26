using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common.Interfaces;

using static IM.Service.Common.Net.Helpers.ServiceHelper;

namespace IM.Service.Market.Services.RestApi.Common;

public class RestQueryQuarterService<TEntity> : IRestQueryService<TEntity> where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly Repository<TEntity> repository;
    public RestQueryQuarterService(Repository<TEntity> repository) => this.repository = repository;

    public IQueryable<TEntity> GetQuery<T>(T filter) where T : class, IFilter<TEntity>
    {
        return repository.GetQuery(filter.Expression);
    }
    public async Task<(IQueryable<TEntity> query, int count)> GetQueryWithCountAsync<T>(T filter, Paginatior pagination) where T : class, IFilter<TEntity>
    {
        var filteredQuery = repository.GetQuery(filter.Expression);
        var count = await repository.GetCountAsync(filteredQuery);
        return (repository.GetPaginationQuery(filteredQuery, pagination, x => x.Year, x => x.Quarter), count);
    }
}