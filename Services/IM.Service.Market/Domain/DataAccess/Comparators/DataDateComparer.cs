using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;

namespace IM.Service.Market.Domain.DataAccess.Comparators;

public class DataDateComparer<T> : IEqualityComparer<T> where T : class, IDataIdentity, IDateIdentity
{
    public bool Equals(T? x, T? y) => (x!.CompanyId, x.SourceId, x.Date) == (y!.CompanyId, y.SourceId, y.Date);
    public int GetHashCode(T obj) => (obj.CompanyId, obj.SourceId, obj.Date).GetHashCode();
}