using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Services.RestApi.Mappers.Interfaces;

public interface IMapperWrite<TEntity, in TPost> where TPost : class where TEntity : class, IDataIdentity
{
    TEntity MapTo(TPost model);
    TEntity MapTo(TEntity id, TPost model);
    TEntity[] MapTo(IEnumerable<TPost> models);
}