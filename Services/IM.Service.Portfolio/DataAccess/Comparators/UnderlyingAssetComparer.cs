using IM.Service.Portfolio.DataAccess.Entities;

using System.Collections.Generic;

namespace IM.Service.Portfolio.DataAccess.Comparators;

public class UnderlyingAssetComparer : IEqualityComparer<UnderlyingAsset>
{
    public bool Equals(UnderlyingAsset? x, UnderlyingAsset? y) => x!.Id == y!.Id;
    public int GetHashCode(UnderlyingAsset obj) => obj.Id.GetHashCode();
}