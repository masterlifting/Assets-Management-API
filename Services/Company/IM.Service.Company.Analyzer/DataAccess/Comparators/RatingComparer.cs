using IM.Service.Company.Analyzer.DataAccess.Entities;
using System.Collections.Generic;

namespace IM.Service.Company.Analyzer.DataAccess.Comparators;

public class RatingComparer : IEqualityComparer<Rating>
{
    public bool Equals(Rating? x, Rating? y) => x is not null && y is not null && x.CompanyId == y.CompanyId;
    public int GetHashCode(Rating obj) => obj.CompanyId.GetHashCode();
}