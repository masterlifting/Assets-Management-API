using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Currency : CommonEntityType
{
    public IEnumerable<Transaction>? Transactions { get; set; }
}