using IM.Service.MarketData.Domain.Entities;

namespace IM.Service.MarketData.Domain.DataAccess.Comparators;

public class FloatComparer : IEqualityComparer<Float>
{
    public bool Equals(Float? x, Float? y) => (x!.CompanyId, x.SourceId, x.Value) == (y!.CompanyId, y.SourceId, y.Value);
    public int GetHashCode(Float obj) => (obj.CompanyId, obj.SourceId, obj.Value).GetHashCode();
}