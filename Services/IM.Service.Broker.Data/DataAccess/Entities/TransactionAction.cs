using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class TransactionAction : CommonEntityType
{
    public TransactionActionType TransactionActionType { get; set; } = null!;
    public byte TransactionActionTypeId { get; set; }
}