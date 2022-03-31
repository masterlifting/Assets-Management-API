using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.RestApi.Common.Interfaces;
using IM.Service.Market.Services.RestApi.Mappers.Interfaces;

namespace IM.Service.Market.Services.RestApi.Common;

public class RestApiRead<TEntity, TGet> where TGet : class where TEntity : class, IDataIdentity, IPeriod
{
    private readonly IRestQueryService<TEntity> queryService;
    private readonly IMapperRead<TEntity, TGet> mapper;

    protected RestApiRead(IRestQueryService<TEntity> queryService, IMapperRead<TEntity, TGet> mapper)
    {
        this.queryService = queryService;
        this.mapper = mapper;
    }

    public async Task<ResponseModel<TGet>> GetAsync(TEntity entity)
    {
        var queryResult = await queryService.GetAsync(entity);

        return queryResult is not null
            ? new() { Data = await mapper.MapFromAsync(queryResult) }
            : new() { Errors = new[] { $"{typeof(TEntity).Name} of '{entity.CompanyId}' not found" } };
    }
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetAsync<T>(T filter, HttpPagination pagination) where T : class, IFilter<TEntity>
    {
        var (query, count) = await queryService.GetQueryWithCountResultAsync(filter, pagination);

        return new()
        {
            Data = new()
            {
                Items = await mapper.MapFromAsync(query),
                Count = count
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<TGet>>> GetLastAsync<T>(T filter, HttpPagination pagination) where T : class, IFilter<TEntity>
    {
        var query = queryService.GetQuery(filter, pagination);

        var result = await mapper.MapLastFromAsync(query);

        return new()
        {
            Data = new()
            {
                Items = pagination.GetPaginatedResult(result),
                Count = result.Length
            }
        };
    }
}
