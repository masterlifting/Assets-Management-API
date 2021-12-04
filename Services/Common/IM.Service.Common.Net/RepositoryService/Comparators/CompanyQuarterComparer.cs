using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Common.Net.RepositoryService.Comparators
{
    public class CompanyQuarterComparer<T> : IEqualityComparer<T> where T : class, ICompanyQuarterIdentity
    {
        public bool Equals(T? x, T? y) => (CompanyId: x!.CompanyId, x.Year, x.Quarter) == (y!.CompanyId, y.Year, y.Quarter);
        public int GetHashCode(T obj) => (CompanyId: obj.CompanyId, obj.Year, obj.Quarter).GetHashCode();
    }
}
