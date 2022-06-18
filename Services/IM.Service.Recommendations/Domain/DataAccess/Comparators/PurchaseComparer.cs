using System.Collections.Generic;
using IM.Service.Recommendations.Domain.Entities;

namespace IM.Service.Recommendations.Domain.DataAccess.Comparators;

    public class PurchaseComparer : IEqualityComparer<Purchase>
{
    public bool Equals(Purchase? x, Purchase? y) => x!.Id != 0 && x.Id == y!.Id;
    public int GetHashCode(Purchase obj) => obj.Id.GetHashCode();
}