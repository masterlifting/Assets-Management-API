using System.Collections.Generic;
using IM.Service.Broker.Data.DataAccess.Entities;

namespace IM.Service.Broker.Data.DataAccess.Comparators;

public class TransactionComparer : IEqualityComparer<Transaction>
{
    public bool Equals(Transaction? x, Transaction? y) => x!.Id != 0 && x.Id == y!.Id;
    public int GetHashCode(Transaction obj) => obj.Id.GetHashCode();
}