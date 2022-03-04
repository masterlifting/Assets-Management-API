using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Interfaces;

namespace IM.Service.Data.Services.RestApi.Mappers.Interfaces;

public interface IMapperRead<in TEntity, TGet> where TGet : class where TEntity : class, IDataIdentity, IPeriod
{
    Task<TGet> MapFromAsync(TEntity entity);
    Task<TGet[]> MapFromAsync(IQueryable<TEntity> query);
    Task<TGet[]> MapLastFromAsync(IQueryable<TEntity> query);
}