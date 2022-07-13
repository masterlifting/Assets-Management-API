using IM.Service.Recommendations.Domain.Entities;

using System.Collections.Generic;

namespace IM.Service.Recommendations.Domain.DataAccess.Comparators;

public class AssetComparer : IEqualityComparer<Asset>
{
    public bool Equals(Asset? x, Asset? y) => (x!.Id, x.TypeId) == (y!.Id, y.TypeId);
    public int GetHashCode(Asset obj) => (obj.Id, obj.TypeId).GetHashCode();
}