using IM.Service.Common.Net.Models.Entity;

using System.Text.Json.Serialization;

namespace IM.Service.Market.Domain.Entities.Catalogs;

public class Sector : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<Industry>? Industries { get; set; }
}