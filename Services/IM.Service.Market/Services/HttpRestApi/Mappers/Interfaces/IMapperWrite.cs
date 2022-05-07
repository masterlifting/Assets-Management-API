using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Services.HttpRestApi.Mappers.Interfaces;

public interface IMapperWrite<TEntity, in TPost> where TPost : class where TEntity : class, IDataIdentity
{
    TEntity MapTo(TEntity id, TPost model);
    TEntity MapTo(string companyId, byte sourceId, TPost model);
    TEntity[] MapTo(string companyId, byte sourceId, IEnumerable<TPost> models);
}