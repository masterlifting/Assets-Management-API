using System.Collections.Generic;
using IM.Service.Company.Analyzer.DataAccess.Entities;

namespace IM.Service.Company.Analyzer.DataAccess.Comparators;

public class RatingDataComparer : IEqualityComparer<RatingData>
{
    public bool Equals(RatingData? x, RatingData? y) => x is not null && y is not null && x.Id == y.Id;
    public int GetHashCode(RatingData obj) => obj.Id.GetHashCode();
}