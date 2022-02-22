using System.Collections.Generic;
using IM.Service.Portfolio.DataAccess.Entities;

namespace IM.Service.Portfolio.DataAccess.Comparators;

public class DealComparer : IEqualityComparer<Deal>
{
    public bool Equals(Deal? x, Deal? y) => x!.Id != 0 && x.Id == y!.Id;
    public int GetHashCode(Deal obj) => obj.Id.GetHashCode();
}