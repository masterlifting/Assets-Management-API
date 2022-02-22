using System.Collections.Generic;
using IM.Service.Market.Analyzer.DataAccess.Entities;

namespace IM.Service.Market.Analyzer.DataAccess.Comparators;

public class RatingComparer : IEqualityComparer<Rating>
{
    public bool Equals(Rating? x, Rating? y) => x!.CompanyId == y!.CompanyId;
    public int GetHashCode(Rating obj) => obj.CompanyId.GetHashCode();
}