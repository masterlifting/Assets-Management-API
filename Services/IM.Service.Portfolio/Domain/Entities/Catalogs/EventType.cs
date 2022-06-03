using IM.Service.Shared.Models.Entity;

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class EventType : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<Event>? Events { get; set; }
    
    public virtual Operation Operation { get; set; } = null!;
    public byte OperationId { get; init; }
}