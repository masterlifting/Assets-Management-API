using IM.Service.Company.Data.DataAccess.Entities.ManyToMany;

using System.Collections.Generic;

namespace IM.Service.Company.Data.DataAccess.Comparators;

public class CompanySourceComparer : IEqualityComparer<CompanySource>
{
    public bool Equals(CompanySource? x, CompanySource? y) => (x!.CompanyId,  x.SourceId) == (y!.CompanyId, y.SourceId);
    public int GetHashCode(CompanySource obj) => (obj.CompanyId, obj.SourceId).GetHashCode();
}