using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class TransactionActionType : CommonEntityType
{
    public IEnumerable<TransactionAction>? TransactionActions { get; set; }
}