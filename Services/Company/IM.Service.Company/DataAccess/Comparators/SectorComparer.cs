using IM.Service.Company.DataAccess.Entities;

using System;
using System.Collections.Generic;

namespace IM.Service.Company.DataAccess.Comparators;

public class SectorComparer : IEqualityComparer<Sector>
{
    public bool Equals(Sector? x, Sector? y) => string.Equals(x!.Name, y!.Name, StringComparison.InvariantCultureIgnoreCase);
    public int GetHashCode(Sector obj) => (obj.Name).GetHashCode();
}