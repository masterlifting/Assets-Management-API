using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Domain.DataAccess.Comparators;

public class CompanySourceComparer : IEqualityComparer<CompanySource>
{
    public bool Equals(CompanySource? x, CompanySource? y) => (x!.CompanyId,  x.SourceId) == (y!.CompanyId, y.SourceId);
    public int GetHashCode(CompanySource obj) => (obj.CompanyId, obj.SourceId).GetHashCode();
}