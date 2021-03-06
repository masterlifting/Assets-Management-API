using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Shared.SqlAccess.Filters;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.Http.Common.Interfaces;
using static IM.Service.Shared.Helpers.ServiceHelper;

namespace IM.Service.Market.Services.Http.Common;

public class RestQueryQuarterService<TEntity> : IRestQueryService<TEntity> where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly Repository<TEntity> repository;
    public RestQueryQuarterService(Repository<TEntity> repository) => this.repository = repository;

    public IQueryable<TEntity> GetQuery<T>(T filter) where T : class, IFilter<TEntity>
    {
        return repository.Where(filter.Expression);
    }
    public async Task<(IQueryable<TEntity> query, int count)> GetQueryWithCountAsync<T>(T filter, Paginatior pagination) where T : class, IFilter<TEntity>
    {
        var filteredQuery = repository.Where(filter.Expression);
        var count = await repository.GetCountAsync(filteredQuery);
        return (repository.GetPaginationQuery(filteredQuery, pagination, x => x.Year, x => x.Quarter), count);
    }
}