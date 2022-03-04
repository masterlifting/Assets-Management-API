using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Interfaces;

namespace IM.Service.Data.Services.RestApi.Mappers.Interfaces;

public interface IMapperWrite<TEntity, in TPost> where TPost : class where TEntity : class, IDataIdentity, IPeriod
{
    TEntity MapTo(TPost model);
    TEntity MapTo(TEntity id, TPost model);
    TEntity[] MapTo(IEnumerable<TPost> models);
}