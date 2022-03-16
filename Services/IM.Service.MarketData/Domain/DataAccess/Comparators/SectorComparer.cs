using IM.Service.MarketData.Domain.Entities.Catalogs;

namespace IM.Service.MarketData.Domain.DataAccess.Comparators;

public class SectorComparer : IEqualityComparer<Sector>
{
    public bool Equals(Sector? x, Sector? y) => string.Equals(x!.Name, y!.Name, StringComparison.OrdinalIgnoreCase);
    public int GetHashCode(Sector obj) => obj.Name.GetHashCode();
}