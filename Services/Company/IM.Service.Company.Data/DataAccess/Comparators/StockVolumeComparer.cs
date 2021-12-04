using System.Collections.Generic;
using IM.Service.Company.Data.DataAccess.Entities;

namespace IM.Service.Company.Data.DataAccess.Comparators
{
    public class StockVolumeComparer : IEqualityComparer<StockVolume>
    {
        public bool Equals(StockVolume? x, StockVolume? y) => (CompanyId: x!.CompanyId, x.Value) == (y!.CompanyId, x.Value);
        public int GetHashCode(StockVolume obj) => (CompanyId: obj.CompanyId, obj.Value).GetHashCode();
    }
}
