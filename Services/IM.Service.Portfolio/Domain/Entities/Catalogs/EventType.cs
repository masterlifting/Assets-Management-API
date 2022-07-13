using IM.Service.Shared.Models.Entity;

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class EventType : Catalog
{
    public virtual OperationType OperationType { get; set; } = null!;
    public byte OperationTypeId { get; set; } = (byte)Enums.OperationTypes.Default;

    [JsonIgnore]
    public virtual IEnumerable<Event>? Events { get; set; }
}