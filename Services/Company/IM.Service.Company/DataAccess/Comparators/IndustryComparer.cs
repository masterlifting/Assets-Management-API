using IM.Service.Company.DataAccess.Entities;

using System;
using System.Collections.Generic;

namespace IM.Service.Company.DataAccess.Comparators;

public class IndustryComparer : IEqualityComparer<Industry>
{
    public bool Equals(Industry? x, Industry? y) => string.Equals(x!.Name, y!.Name, StringComparison.InvariantCultureIgnoreCase);
    public int GetHashCode(Industry obj) => (obj.Name).GetHashCode();
}