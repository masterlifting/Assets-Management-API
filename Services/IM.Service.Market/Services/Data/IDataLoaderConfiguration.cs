using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Services.Data;

public interface IDataLoaderConfiguration<TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    public Func<TEntity, bool> IsCurrentDataCondition { get; }
    public IEqualityComparer<TEntity> Comparer { get; }
    public ILastDataHelper<TEntity> LastDataHelper { get; }
    public DataGrabber<TEntity> Grabber { get; }
}