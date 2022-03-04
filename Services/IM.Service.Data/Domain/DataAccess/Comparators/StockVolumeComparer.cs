using IM.Service.Data.Domain.Entities;

namespace IM.Service.Data.Domain.DataAccess.Comparators;

public class StockVolumeComparer : IEqualityComparer<Float>
{
    public bool Equals(Float? x, Float? y) => (x!.CompanyId, x.Value) == (y!.CompanyId, y.Value);
    public int GetHashCode(Float obj) => (obj.CompanyId, obj.Value).GetHashCode();
}