using System.Collections.Generic;
using IM.Service.Market.DataAccess.Entities;

namespace IM.Service.Market.DataAccess.Comparators;

public class StockVolumeComparer : IEqualityComparer<StockVolume>
{
    public bool Equals(StockVolume? x, StockVolume? y) => (x!.CompanyId, x.Value) == (y!.CompanyId, y.Value);
    public int GetHashCode(StockVolume obj) => (obj.CompanyId, obj.Value).GetHashCode();
}