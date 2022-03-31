using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Domain.DataAccess.Comparators;

public class FloatComparer : IEqualityComparer<Float>
{
    public bool Equals(Float? x, Float? y) => (x!.CompanyId, x.SourceId, x.Value) == (y!.CompanyId, y.SourceId, y.Value);
    public int GetHashCode(Float obj) => (obj.CompanyId, obj.SourceId, obj.Value).GetHashCode();
}