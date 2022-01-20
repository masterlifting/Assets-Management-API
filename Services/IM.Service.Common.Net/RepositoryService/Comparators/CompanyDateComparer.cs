using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

namespace IM.Service.Common.Net.RepositoryService.Comparators;

public class CompanyDateComparer<T> : IEqualityComparer<T> where T : class, ICompanyDateIdentity
{
    public bool Equals(T? x, T? y) => (CompanyId: x!.CompanyId, x.Date) == (y!.CompanyId, y.Date);
    public int GetHashCode(T obj) => (CompanyId: obj.CompanyId, obj.Date).GetHashCode();
}