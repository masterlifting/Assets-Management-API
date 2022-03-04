using IM.Service.Data.Domain.Entities.ManyToMany;

namespace IM.Service.Data.Domain.DataAccess.Comparators;

public class CompanySourceComparer : IEqualityComparer<CompanySource>
{
    public bool Equals(CompanySource? x, CompanySource? y) => (x!.CompanyId,  x.SourceId) == (y!.CompanyId, y.SourceId);
    public int GetHashCode(CompanySource obj) => (obj.CompanyId, obj.SourceId).GetHashCode();
}