using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Shared.SqlAccess.Filters;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Services.Http.Mappers.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.Http.Common;

public class RestApiWrite<TEntity, TPost> where TPost : class where TEntity : class, IDataIdentity, IPeriod
{
    private readonly Repository<TEntity> repository;
    private readonly IMapperWrite<TEntity, TPost> mapper;

    public RestApiWrite(Repository<TEntity> repository, IMapperWrite<TEntity, TPost> mapper)
    {
        this.mapper = mapper;
        this.repository = repository;
    }

    public async Task<TEntity> CreateAsync(string companyId, int sourceId, TPost model)
    {
        var entity = mapper.MapTo(companyId.ToUpperInvariant(), (byte)sourceId, model);
        return await repository.CreateAsync(entity, entity.CompanyId);
    }
    public async Task<TEntity[]> CreateAsync(string companyId, int sourceId, IEnumerable<TPost> models, IEqualityComparer<TEntity> comparer)
    {
        var entities = mapper.MapTo(companyId.ToUpperInvariant(), (byte)sourceId, models);
        return await repository.CreateRangeAsync(entities, comparer, $"{nameof(companyId)}: {companyId}, {nameof(sourceId)}: {sourceId}");
    }

    public async Task<TEntity> UpdateAsync(TEntity id, TPost model)
    {
        var objectId = id.GetType().GetProperties().Select(x => x.GetValue(id)).ToArray();

        if (objectId is null)
            throw new NullReferenceException("id not recognized");

        var entity = mapper.MapTo(id, model);
        return await repository.UpdateAsync(objectId!, entity, entity.CompanyId);
    }

    public async Task<object[]> DeleteAsync<T>(T filter) where T : class, IFilter<TEntity>
    {
        var entities = await repository.Where(filter.Expression).ToArrayAsync();
        var deletedEntities = await repository.DeleteRangeAsync(entities, string.Join("; ", entities.Select(x => x.CompanyId).Distinct()));
        return deletedEntities.Select(x => new { x.CompanyId, x.SourceId }).ToArray();
    }
}
