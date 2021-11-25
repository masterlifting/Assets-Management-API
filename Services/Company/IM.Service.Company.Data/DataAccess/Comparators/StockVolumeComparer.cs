using System.Collections.Generic;
using IM.Service.Company.Data.DataAccess.Entities;

namespace IM.Service.Company.Data.DataAccess.Comparators
{
    public class StockVolumeComparer : IEqualityComparer<StockVolume>
    {
        public bool Equals(StockVolume? x, StockVolume? y) => (x!.CompanyId, x.Value) == (y!.CompanyId, x.Value);
        public int GetHashCode(StockVolume obj) => (obj.CompanyId, obj.Value).GetHashCode();
    }
}
