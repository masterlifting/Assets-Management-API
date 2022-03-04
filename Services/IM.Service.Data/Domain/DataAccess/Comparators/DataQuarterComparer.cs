﻿using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Data.Domain.Entities.Interfaces;

namespace IM.Service.Data.Domain.DataAccess.Comparators;

public class DataQuarterComparer<T> : IEqualityComparer<T> where T : class, IDataIdentity, IQuarterIdentity
{
    public bool Equals(T? x, T? y) => (x!.CompanyId, x.SourceId, x.Year, x.Quarter) == (y!.CompanyId, y.SourceId, y.Year, y.Quarter);
    public int GetHashCode(T obj) => (obj.CompanyId, obj.SourceId, obj.Year, obj.Quarter).GetHashCode();
}