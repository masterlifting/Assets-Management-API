using System.Collections.Generic;
using System.Text.Json.Serialization;
using IM.Service.Shared.Models.Entity;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class Exchange : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<Deal>? Deals { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Event>? Events { get; set; }
}