using System.Collections.Generic;
using System.Text.Json.Serialization;
using IM.Service.Shared.Models.Entity;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class OperationType : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<EventType>? EventTypes { get; set; }
}