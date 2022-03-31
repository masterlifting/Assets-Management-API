using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Domain.DataAccess.Comparators;

public class CompanyComparer : IEqualityComparer<Company>
{
    public bool Equals(Company? x, Company? y) => string.Equals(x!.Id, y!.Id, StringComparison.OrdinalIgnoreCase);
    public int GetHashCode(Company obj) => obj.Id.GetHashCode();
}