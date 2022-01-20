using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class TransactionActionType : CommonEntityType
{
    public IEnumerable<TransactionAction>? TransactionActions { get; set; }
}