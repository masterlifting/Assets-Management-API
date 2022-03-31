using IM.Service.Market.Domain.Entities;

namespace IM.Service.Market.Domain.DataAccess.Comparators;

public class RatingComparer : IEqualityComparer<Rating>
{
    public bool Equals(Rating? x, Rating? y) => x!.CompanyId == y!.CompanyId;
    public int GetHashCode(Rating obj) => obj.CompanyId.GetHashCode();
}