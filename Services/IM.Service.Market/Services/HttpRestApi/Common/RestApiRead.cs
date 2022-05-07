using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.Models.Services;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.HttpRestApi.Common.Interfaces;
using IM.Service.Market.Services.HttpRestApi.Mappers.Interfaces;
using static IM.Service.Common.Net.Helpers.ServiceHelper;

namespace IM.Service.Market.Services.HttpRestApi.Common;

public class RestApiRead<TEntity, TGet> where TGet : class where TEntity : class, IDataIdentity, IPeriod
{
    private readonly IRestQueryService<TEntity> queryService;
    private readonly IMapperRead<TEntity, TGet> mapper;

    public RestApiRead(IRestQueryService<TEntity> queryService, IMapperRead<TEntity, TGet> mapper)
    {
        this.queryService = queryService;
        this.mapper = mapper;
    }

    public async Task<PaginationModel<TGet>> GetAsync<T>(T filter, Paginatior pagination) where T : class, IFilter<TEntity>
    {
        var (query, count) = await queryService.GetQueryWithCountAsync(filter, pagination);

        return new PaginationModel<TGet> { Items = await mapper.MapFromAsync(query), Count = count };
    }
    public async Task<PaginationModel<TGet>> GetLastAsync<T>(T filter, Paginatior pagination) where T : class, IFilter<TEntity>
    {
        var query = queryService.GetQuery(filter);

        var lastResult = await mapper.MapLastFromAsync(query);

        return new PaginationModel<TGet> { Items = pagination.GetPaginatedResult(lastResult), Count = lastResult.Length };
    }
}
