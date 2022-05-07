using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Services.HttpRestApi.Mappers.Interfaces;

public interface IMapperRead<in TEntity, TGet> where TGet : class where TEntity : class, IDataIdentity
{
    Task<TGet[]> MapFromAsync(IQueryable<TEntity> query);
    Task<TGet[]> MapLastFromAsync(IQueryable<TEntity> query);
}