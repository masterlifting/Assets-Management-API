using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.Entities.Interfaces;
using IM.Service.Data.Services.RestApi.Mappers.Interfaces;

namespace IM.Service.Data.Services.RestApi.Common;

public class RestMethodWrite<TEntity, TPost> where TPost : class where TEntity : class, IDataIdentity, IPeriod
{
    private readonly Repository<TEntity> repository;
    private readonly IMapperWrite<TEntity, TPost> mapper;

    protected RestMethodWrite(Repository<TEntity> repository, IMapperWrite<TEntity, TPost> mapper)
    {
        this.mapper = mapper;
        this.repository = repository;
    }

    public async Task<(string? error, TEntity? entity)> CreateAsync(TPost model)
    {
        var entity = mapper.MapTo(model);

        var message = $"{typeof(TEntity).Name} of '{entity.CompanyId}' create";

        return await repository.CreateAsync(entity, message);
    }
    public async Task<(string? error, TEntity[]? entities)> CreateAsync(IEnumerable<TPost> models, IEqualityComparer<TEntity> comparer)
    {
        var entities = mapper.MapTo(models);

        return await repository.CreateAsync(entities, comparer, $"Source count: {entities.Length}");
    }
    public async Task<(string? error, TEntity? entity)> UpdateAsync(TEntity id, TPost model)
    {
        var entity = mapper.MapTo(id, model);

        var info = $"{typeof(TEntity)} of '{entity.CompanyId}' update";

        return await repository.UpdateAsync(entity, info);
    }
    public async Task<(string? error, TEntity? entity)> DeleteAsync(TEntity id)
    {
        var info = $"{typeof(TEntity).Name} of '{id.CompanyId}' delete";

        return await repository.DeleteAsync(id, info);
    }
}