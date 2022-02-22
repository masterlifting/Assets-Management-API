using System.Collections.Generic;
using IM.Service.Market.DataAccess.Entities.ManyToMany;

namespace IM.Service.Market.DataAccess.Comparators;

public class CompanySourceComparer : IEqualityComparer<CompanySource>
{
    public bool Equals(CompanySource? x, CompanySource? y) => (x!.CompanyId,  x.SourceId) == (y!.CompanyId, y.SourceId);
    public int GetHashCode(CompanySource obj) => (obj.CompanyId, obj.SourceId).GetHashCode();
}