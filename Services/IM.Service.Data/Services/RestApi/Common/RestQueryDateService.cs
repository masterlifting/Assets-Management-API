﻿using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.Entities.Interfaces;
using IM.Service.Data.Services.RestApi.Common.Interfaces;

namespace IM.Service.Data.Services.RestApi.Common;

public class RestQueryDateService<TEntity> : IRestQueryService<TEntity> where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly Repository<TEntity> repository;
    public RestQueryDateService(Repository<TEntity> repository) => this.repository = repository;

    public Task<TEntity?> GetAsync(TEntity entity) => repository.FindAsync(entity);
    public IQueryable<TEntity> GetQuery<T>(T filter, HttpPagination pagination) where T : class, IFilter<TEntity>
    {
        var filteredQuery = repository.GetQuery(filter.Expression);
        return repository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);
    }
    public async Task<(IQueryable<TEntity> query, int count)> GetQueryWithCountResultAsync<T>(T filter, HttpPagination pagination) where T : class, IFilter<TEntity>
    {
        var filteredQuery = repository.GetQuery(filter.Expression);
        var count = await repository.GetCountAsync(filteredQuery);
        return (repository.GetPaginationQuery(filteredQuery, pagination, x => x.Date), count);
    }
}