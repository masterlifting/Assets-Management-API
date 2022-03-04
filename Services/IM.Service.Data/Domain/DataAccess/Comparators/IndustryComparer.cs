using IM.Service.Data.Domain.Entities.Catalogs;

namespace IM.Service.Data.Domain.DataAccess.Comparators;

public class IndustryComparer : IEqualityComparer<Industry>
{
    public bool Equals(Industry? x, Industry? y) => string.Equals(x!.Name, y!.Name, StringComparison.OrdinalIgnoreCase);
    public int GetHashCode(Industry obj) => obj.Name.GetHashCode();
}