using IM.Service.MarketData.Domain.Entities.Catalogs;

namespace IM.Service.MarketData.Domain.DataAccess.Comparators;

public class IndustryComparer : IEqualityComparer<Industry>
{
    public bool Equals(Industry? x, Industry? y) => string.Equals(x!.Name, y!.Name, StringComparison.OrdinalIgnoreCase);
    public int GetHashCode(Industry obj) => obj.Name.GetHashCode();
}