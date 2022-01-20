using IM.Service.Company.Data.DataAccess.Entities.ManyToMany;

using System.Collections.Generic;

namespace IM.Service.Company.Data.DataAccess.Comparators;

public class CompanySourceComparer : IEqualityComparer<CompanySource>
{
    public bool Equals(CompanySource? x, CompanySource? y) => (x!.CompanyId,  x.SourceId, x.Value) == (y!.CompanyId, y.SourceId, y.Value);
    public int GetHashCode(CompanySource obj) => (obj.CompanyId, obj.SourceId, obj.Value).GetHashCode();
}