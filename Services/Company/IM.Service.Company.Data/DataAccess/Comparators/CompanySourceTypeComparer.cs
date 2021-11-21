using IM.Service.Company.Data.DataAccess.Entities;

using System.Collections.Generic;

namespace IM.Service.Company.Data.DataAccess.Comparators;

public class CompanySourceTypeComparer : IEqualityComparer<CompanySourceType>
{
    public bool Equals(CompanySourceType? x, CompanySourceType? y) => (x!.CompanyId, x.SourceTypeId, x.Value) == (y!.CompanyId, y.SourceTypeId, y.Value);
    public int GetHashCode(CompanySourceType obj) => (obj.CompanyId, obj.SourceTypeId, obj.Value).GetHashCode();
}