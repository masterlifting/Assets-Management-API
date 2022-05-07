using System.Collections.Generic;
using IM.Service.Portfolio.Domain.Entities;

namespace IM.Service.Portfolio.Domain.DataAccess.Comparators;

public class UnderlyingAssetComparer : IEqualityComparer<UnderlyingAsset>
{
    public bool Equals(UnderlyingAsset? x, UnderlyingAsset? y) => x!.Id == y!.Id;
    public int GetHashCode(UnderlyingAsset obj) => obj.Id.GetHashCode();
}