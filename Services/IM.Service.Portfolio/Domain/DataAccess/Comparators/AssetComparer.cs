using System.Collections.Generic;

using IM.Service.Portfolio.Domain.Entities;

namespace IM.Service.Portfolio.Domain.DataAccess.Comparators;

public class AssetComparer : IEqualityComparer<Asset>
{
    public bool Equals(Asset? x, Asset? y) => (x!.Id, x.TypeId) == (y!.Id, y.TypeId);
    public int GetHashCode(Asset obj) => (obj.Id, obj.TypeId).GetHashCode();
}