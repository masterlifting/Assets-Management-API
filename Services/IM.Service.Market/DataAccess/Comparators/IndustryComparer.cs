using System;
using System.Collections.Generic;
using IM.Service.Market.DataAccess.Entities;

namespace IM.Service.Market.DataAccess.Comparators;

public class IndustryComparer : IEqualityComparer<Industry>
{
    public bool Equals(Industry? x, Industry? y) => string.Equals(x!.Name, y!.Name, StringComparison.OrdinalIgnoreCase);
    public int GetHashCode(Industry obj) => obj.Name.GetHashCode();
}